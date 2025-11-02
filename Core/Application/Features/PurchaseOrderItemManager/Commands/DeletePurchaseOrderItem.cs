using Application.Common.Extensions;
using Application.Common.Repositories;
using Application.Features.InventoryTransactionManager;
using Application.Features.PurchaseOrderManager;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.PurchaseOrderItemManager.Commands;

public class DeletePurchaseOrderItemResult
{
    public PurchaseOrderItem? Data { get; set; }
}

public class DeletePurchaseOrderItemRequest : IRequest<DeletePurchaseOrderItemResult>
{
    public string? Id { get; init; }
    public string? DeletedById { get; init; }
}

public class DeletePurchaseOrderItemValidator : AbstractValidator<DeletePurchaseOrderItemRequest>
{
    public DeletePurchaseOrderItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeletePurchaseOrderItemHandler : IRequestHandler<DeletePurchaseOrderItemRequest, DeletePurchaseOrderItemResult>
{
    private readonly ICommandRepository<PurchaseOrderItem> _repository;
    private readonly ICommandRepository<GoodsReceive> _goodsReceiveRepository;
    private readonly ICommandRepository<InventoryTransaction> _inventoryTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PurchaseOrderService _purchaseOrderService;

    public DeletePurchaseOrderItemHandler(
        ICommandRepository<PurchaseOrderItem> repository,
        ICommandRepository<GoodsReceive> goodsReceiveRepository,
        ICommandRepository<InventoryTransaction> inventoryTransactionRepository,
        IUnitOfWork unitOfWork,
        PurchaseOrderService purchaseOrderService
    )
    {
        _repository = repository;
        _goodsReceiveRepository = goodsReceiveRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _unitOfWork = unitOfWork;
        _purchaseOrderService = purchaseOrderService;
    }

    public async Task<DeletePurchaseOrderItemResult> Handle(DeletePurchaseOrderItemRequest request, CancellationToken cancellationToken)
    {

        var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);

        if (entity == null)
        {
            throw new Exception($"Entity not found: {request.Id}");
        }

        entity.UpdatedById = request.DeletedById;

        _repository.Delete(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        _purchaseOrderService.Recalculate(entity.PurchaseOrderId ?? "");

        // Find related GoodsReceive and InventoryTransactions
        var goodsReceive = await _goodsReceiveRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == entity.PurchaseOrderId, cancellationToken);

        if (goodsReceive != null)
        {
            // Find and delete related inventory transactions for this product
            var inventoryTransactions = await _inventoryTransactionRepository
                .GetQuery()
                .ApplyIsDeletedFilter(false)
                .Where(x => x.ModuleId == goodsReceive.Id &&
                           x.ModuleName == nameof(GoodsReceive) &&
                           x.ProductId == entity.ProductId)
                .ToListAsync(cancellationToken);

            foreach (var transaction in inventoryTransactions)
            {
                transaction.UpdatedById = request.DeletedById;
                _inventoryTransactionRepository.Delete(transaction);
            }

            // Check if this was the last PurchaseOrderItem for this GoodsReceive
            var remainingItems = await _repository
                .GetQuery()
                .ApplyIsDeletedFilter(false)
                .Where(x => x.PurchaseOrderId == entity.PurchaseOrderId)
                .CountAsync(cancellationToken);

            // If this was the last item, delete the GoodsReceive too
            if (remainingItems <= 1) // Current item is still counted until we save
            {
                goodsReceive.UpdatedById = request.DeletedById;
                _goodsReceiveRepository.Delete(goodsReceive);
            }

            await _unitOfWork.SaveAsync(cancellationToken);
        }

        return new DeletePurchaseOrderItemResult
        {
            Data = entity
        };
    }
}


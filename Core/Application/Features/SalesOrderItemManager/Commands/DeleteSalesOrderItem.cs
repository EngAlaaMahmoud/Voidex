using Application.Common.Extensions;
using Application.Common.Repositories;
using Application.Features.InventoryTransactionManager;
using Application.Features.SalesOrderManager;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SalesOrderItemManager.Commands;

public class DeleteSalesOrderItemResult
{
    public SalesOrderItem? Data { get; set; }
}

public class DeleteSalesOrderItemRequest : IRequest<DeleteSalesOrderItemResult>
{
    public string? Id { get; init; }
    public string? DeletedById { get; init; }
}

public class DeleteSalesOrderItemValidator : AbstractValidator<DeleteSalesOrderItemRequest>
{
    public DeleteSalesOrderItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteSalesOrderItemHandler : IRequestHandler<DeleteSalesOrderItemRequest, DeleteSalesOrderItemResult>
{
    private readonly ICommandRepository<SalesOrderItem> _repository;
    private readonly ICommandRepository<DeliveryOrder> _deliveryOrderRepository;
    private readonly ICommandRepository<InventoryTransaction> _inventoryTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SalesOrderService _salesOrderService;
    private readonly InventoryTransactionService _inventoryTransactionService;

    public DeleteSalesOrderItemHandler(
        ICommandRepository<SalesOrderItem> repository,
        IUnitOfWork unitOfWork,
        SalesOrderService salesOrderService,
        ICommandRepository<DeliveryOrder> deliveryOrderRepository,
        ICommandRepository<InventoryTransaction> inventoryTransactionRepository,
        InventoryTransactionService inventoryTransactionService
        )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _salesOrderService = salesOrderService;
        _deliveryOrderRepository = deliveryOrderRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _inventoryTransactionService = inventoryTransactionService;
    }
    public async Task<DeleteSalesOrderItemResult> Handle(DeleteSalesOrderItemRequest request, CancellationToken cancellationToken)
    {

        var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);

        if (entity == null)
        {
            throw new Exception($"Entity not found: {request.Id}");
        }

        entity.UpdatedById = request.DeletedById;

        _repository.Delete(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        _salesOrderService.Recalculate(entity.SalesOrderId ?? "");
               
        // Find related DeliveryOrder and InventoryTransactions
        var deliveryOrder = await _deliveryOrderRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .FirstOrDefaultAsync(x => x.SalesOrderId == entity.SalesOrderId, cancellationToken);

        if (deliveryOrder != null)
        {
            // Find and delete related inventory transactions for this product
            var inventoryTransactions = await _inventoryTransactionRepository
                .GetQuery()
                .ApplyIsDeletedFilter(false)
                .Where(x => x.ModuleId == deliveryOrder.Id &&
                           x.ModuleName == nameof(DeliveryOrder) &&
                           x.ProductId == entity.ProductId)
                .ToListAsync(cancellationToken);

            foreach (var transaction in inventoryTransactions)
            {
                transaction.UpdatedById = request.DeletedById;
                _inventoryTransactionRepository.Delete(transaction);
            }

            // Check if this was the last SalesOrderItem for this DeliveryOrder
            var remainingItems = await _repository
                .GetQuery()
                .ApplyIsDeletedFilter(false)
                .Where(x => x.SalesOrderId == entity.SalesOrderId)
                .CountAsync(cancellationToken);

            // If this was the last item, delete the DeliveryOrder too
            if (remainingItems <= 1) // Current item is still counted until we save
            {
                deliveryOrder.UpdatedById = request.DeletedById;
                _deliveryOrderRepository.Delete(deliveryOrder);
            }
        }

        return new DeleteSalesOrderItemResult
        {
            Data = entity
        };
    }
}


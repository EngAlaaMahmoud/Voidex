using Application.Common.Extensions;
using Application.Common.Repositories;
using Application.Features.InventoryTransactionManager;
using Application.Features.PurchaseOrderManager;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.PurchaseOrderItemManager.Commands;

public class UpdatePurchaseOrderItemResult
{
    public PurchaseOrderItem? Data { get; set; }
}

public class UpdatePurchaseOrderItemRequest : IRequest<UpdatePurchaseOrderItemResult>
{
    public string? Id { get; init; }
    public string? PurchaseOrderId { get; init; }
    public string? ProductId { get; init; }
    public string? Summary { get; init; }
    public double? UnitPrice { get; init; }
    public double? Quantity { get; init; }
    public string? UpdatedById { get; init; }
}

public class UpdatePurchaseOrderItemValidator : AbstractValidator<UpdatePurchaseOrderItemRequest>
{
    public UpdatePurchaseOrderItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UnitPrice).NotEmpty();
        RuleFor(x => x.Quantity).NotEmpty();
    }
}

public class UpdatePurchaseOrderItemHandler : IRequestHandler<UpdatePurchaseOrderItemRequest, UpdatePurchaseOrderItemResult>
{
    private readonly ICommandRepository<PurchaseOrderItem> _repository;
    private readonly ICommandRepository<GoodsReceive> _goodsReceiveRepository;
    private readonly ICommandRepository<InventoryTransaction> _inventoryTransactionRepository;
    private readonly ICommandRepository<Product> _productRepository;
    private readonly ICommandRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PurchaseOrderService _purchaseOrderService;
    private readonly InventoryTransactionService _inventoryTransactionService;

    public UpdatePurchaseOrderItemHandler(
        ICommandRepository<PurchaseOrderItem> repository,
        ICommandRepository<GoodsReceive> goodsReceiveRepository,
        ICommandRepository<InventoryTransaction> inventoryTransactionRepository,
        ICommandRepository<Product> productRepository,
        ICommandRepository<Warehouse> warehouseRepository,
        IUnitOfWork unitOfWork,
        PurchaseOrderService purchaseOrderService,
        InventoryTransactionService inventoryTransactionService
    )
    {
        _repository = repository;
        _goodsReceiveRepository = goodsReceiveRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
        _purchaseOrderService = purchaseOrderService;
        _inventoryTransactionService = inventoryTransactionService;
    }



    public async Task<UpdatePurchaseOrderItemResult> Handle(UpdatePurchaseOrderItemRequest request, CancellationToken cancellationToken)
    {

        var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);

        if (entity == null)
        {
            throw new Exception($"Entity not found: {request.Id}");
        }

        entity.UpdatedById = request.UpdatedById;

        entity.PurchaseOrderId = request.PurchaseOrderId;
        entity.ProductId = request.ProductId;
        entity.Summary = request.Summary;
        entity.UnitPrice = request.UnitPrice;
        entity.Quantity = request.Quantity;

        entity.Total = entity.UnitPrice * entity.Quantity;

        _repository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        _purchaseOrderService.Recalculate(entity.PurchaseOrderId ?? "");

        // Store old values for comparison
        var oldProductId = entity.ProductId;
        var oldQuantity = entity.Quantity;
        // Update inventory transactions
        await UpdateInventoryTransactions(
            entity,
            oldProductId,
            oldQuantity,
            request.UpdatedById,
            cancellationToken
        );



        return new UpdatePurchaseOrderItemResult
        {
            Data = entity
        };
    }

    private async Task UpdateInventoryTransactions(
       PurchaseOrderItem purchaseOrderItem,
       string? oldProductId,
       double? oldQuantity,
       string? updatedById,
       CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetAsync(
            purchaseOrderItem.ProductId ?? string.Empty,
            cancellationToken
        );

        if (product == null || !product.Physical.GetValueOrDefault())
        {
            return;
        }

        var goodsReceive = await _goodsReceiveRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == purchaseOrderItem.PurchaseOrderId, cancellationToken);

        if (goodsReceive == null)
        {
            return;
        }

        var defaultWarehouse = await _warehouseRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .Where(x => (bool)!x.SystemWarehouse)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultWarehouse == null)
        {
            return;
        }

        // Find existing inventory transaction for this product
        var existingTransaction = await _inventoryTransactionRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .FirstOrDefaultAsync(x =>
                x.ModuleId == goodsReceive.Id &&
                x.ModuleName == nameof(GoodsReceive) &&
                x.ProductId == oldProductId,
                cancellationToken
            );

        if (existingTransaction != null)
        {
            if (oldProductId != purchaseOrderItem.ProductId)
            {
                // Product changed - delete old transaction and create new one
                existingTransaction.UpdatedById = updatedById;
                _inventoryTransactionRepository.Delete(existingTransaction);

                // Create new transaction with new product
                await _inventoryTransactionService.GoodsReceiveCreateInvenTrans(
                    goodsReceive.Id,
                    defaultWarehouse.Id,
                    purchaseOrderItem.ProductId,
                    purchaseOrderItem.Quantity,
                    updatedById,
                    cancellationToken
                );
            }
            else
            {
                // Same product, just update quantity
                await _inventoryTransactionService.GoodsReceiveUpdateInvenTrans(
                    existingTransaction.Id,
                    defaultWarehouse.Id,
                    purchaseOrderItem.ProductId,
                    purchaseOrderItem.Quantity,
                    updatedById,
                    cancellationToken
                );
            }
        }
        else
        {
            // No existing transaction found, create new one
            await _inventoryTransactionService.GoodsReceiveCreateInvenTrans(
                goodsReceive.Id,
                defaultWarehouse.Id,
                purchaseOrderItem.ProductId,
                purchaseOrderItem.Quantity,
                updatedById,
                cancellationToken
            );
        }
    }
}


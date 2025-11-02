using Application.Common.Extensions;
using Application.Common.Repositories;
using Application.Features.InventoryTransactionManager;
using Application.Features.SalesOrderManager;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SalesOrderItemManager.Commands;

public class UpdateSalesOrderItemResult
{
    public SalesOrderItem? Data { get; set; }
}

public class UpdateSalesOrderItemRequest : IRequest<UpdateSalesOrderItemResult>
{
    public string? Id { get; init; }
    public string? SalesOrderId { get; init; }
    public string? ProductId { get; init; }
    public string? Summary { get; init; }
    public double? UnitPrice { get; init; }
    public double? Quantity { get; init; }
    public string? UpdatedById { get; init; }
}

public class UpdateSalesOrderItemValidator : AbstractValidator<UpdateSalesOrderItemRequest>
{
    public UpdateSalesOrderItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UnitPrice).NotEmpty();
        RuleFor(x => x.Quantity).NotEmpty();
    }
}

public class UpdateSalesOrderItemHandler : IRequestHandler<UpdateSalesOrderItemRequest, UpdateSalesOrderItemResult>
{
    private readonly ICommandRepository<SalesOrderItem> _repository;
    private readonly ICommandRepository<DeliveryOrder> _deliveryOrderRepository;
    private readonly ICommandRepository<InventoryTransaction> _inventoryTransactionRepository;
    private readonly ICommandRepository<Product> _productRepository;
    private readonly ICommandRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SalesOrderService _salesOrderService;
    private readonly InventoryTransactionService _inventoryTransactionService;

    public UpdateSalesOrderItemHandler(
        ICommandRepository<SalesOrderItem> repository,
        IUnitOfWork unitOfWork,
        SalesOrderService salesOrderService,
        ICommandRepository<DeliveryOrder> deliveryOrderRepository,
        ICommandRepository<InventoryTransaction> inventoryTransactionRepository,
        ICommandRepository<Product> productRepository,
        ICommandRepository<Warehouse> warehouseRepository,
        InventoryTransactionService inventoryTransactionService
        )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _salesOrderService = salesOrderService;
        _deliveryOrderRepository = deliveryOrderRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryTransactionService = inventoryTransactionService;
    }

    public async Task<UpdateSalesOrderItemResult> Handle(UpdateSalesOrderItemRequest request, CancellationToken cancellationToken)
    {

        var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);

        if (entity == null)
        {
            throw new Exception($"Entity not found: {request.Id}");
        }

        entity.UpdatedById = request.UpdatedById;

        entity.SalesOrderId = request.SalesOrderId;
        entity.ProductId = request.ProductId;
        entity.Summary = request.Summary;
        entity.UnitPrice = request.UnitPrice;
        entity.Quantity = request.Quantity;

        entity.Total = entity.UnitPrice * entity.Quantity;

        _repository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        _salesOrderService.Recalculate(entity.SalesOrderId ?? "");
        // Store old values for comparison
        var oldProductId = entity.ProductId;
        var oldQuantity = entity.Quantity;

        await UpdateInventoryTransactions(entity, oldProductId, oldQuantity, request.UpdatedById, cancellationToken);
        return new UpdateSalesOrderItemResult
        {
            Data = entity
        };
    }
    private async Task UpdateInventoryTransactions(
       SalesOrderItem salesOrderItem,
       string? oldProductId,
       double? oldQuantity,
       string? updatedById,
       CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetAsync(salesOrderItem.ProductId ?? string.Empty, cancellationToken);
        if (product == null || !product.Physical.GetValueOrDefault())
        {
            return;
        }

        var deliveryOrder = await _deliveryOrderRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderItem.SalesOrderId, cancellationToken);

        if (deliveryOrder == null)
        {
            // Create new DeliveryOrder if it doesn't exist
            // (You might need to inject NumberSequenceService and SalesOrderRepository similar to CreateSalesOrderItemHandler)
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
                x.ModuleId == deliveryOrder.Id &&
                x.ModuleName == nameof(DeliveryOrder) &&
                x.ProductId == oldProductId,
            cancellationToken);

        if (existingTransaction != null)
        {
            if (oldProductId != salesOrderItem.ProductId)
            {
                // Product changed - delete old transaction and create new one
                existingTransaction.UpdatedById = updatedById;
                _inventoryTransactionRepository.Delete(existingTransaction);

                // Create new transaction with new product
                await _inventoryTransactionService.DeliveryOrderCreateInvenTrans(
                    deliveryOrder.Id,
                    defaultWarehouse.Id,
                    salesOrderItem.ProductId,
                    salesOrderItem.Quantity,
                    updatedById,
                    cancellationToken
                );
            }
            else
            {
                // Same product, just update quantity
                await _inventoryTransactionService.DeliveryOrderUpdateInvenTrans(
                    existingTransaction.Id,
                    defaultWarehouse.Id,
                    salesOrderItem.ProductId,
                    salesOrderItem.Quantity,
                    updatedById,
                    cancellationToken
                );
            }
        }
        else
        {
            // No existing transaction found, create new one
            await _inventoryTransactionService.DeliveryOrderCreateInvenTrans(
                deliveryOrder.Id,
                defaultWarehouse.Id,
                salesOrderItem.ProductId,
                salesOrderItem.Quantity,
                updatedById,
                cancellationToken
            );
        }
    }
}


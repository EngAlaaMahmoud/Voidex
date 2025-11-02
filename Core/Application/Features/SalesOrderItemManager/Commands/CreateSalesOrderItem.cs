using Application.Common.Extensions;
using Application.Common.Repositories;
using Application.Features.InventoryTransactionManager;
using Application.Features.NumberSequenceManager;
using Application.Features.SalesOrderManager;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SalesOrderItemManager.Commands;

public class CreateSalesOrderItemResult
{
    public SalesOrderItem? Data { get; set; }
}

public class CreateSalesOrderItemRequest : IRequest<CreateSalesOrderItemResult>
{
    public string? SalesOrderId { get; init; }
    public string? ProductId { get; init; }
    public string? Summary { get; init; }
    public double? UnitPrice { get; init; }
    public double? Quantity { get; init; }
    public string? CreatedById { get; init; }
}

public class CreateSalesOrderItemValidator : AbstractValidator<CreateSalesOrderItemRequest>
{
    public CreateSalesOrderItemValidator()
    {
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UnitPrice).NotEmpty();
        RuleFor(x => x.Quantity).NotEmpty();
    }
}

public class CreateSalesOrderItemHandler : IRequestHandler<CreateSalesOrderItemRequest, CreateSalesOrderItemResult>
{
    private readonly ICommandRepository<SalesOrderItem> _repository;
    private readonly ICommandRepository<DeliveryOrder> _deliveryOrderRepository;
    private readonly ICommandRepository<Warehouse> _warehouseRepository;
    private readonly ICommandRepository<Product> _productRepository;
    private readonly ICommandRepository<SalesOrder> _salesOrderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SalesOrderService _salesOrderService;
    private readonly InventoryTransactionService _inventoryTransactionService;
    private readonly NumberSequenceService _numberSequenceService;

    public CreateSalesOrderItemHandler(
        ICommandRepository<SalesOrderItem> repository,
        IUnitOfWork unitOfWork,
        SalesOrderService salesOrderService,
        ICommandRepository<DeliveryOrder> deliveryOrderRepository,
        ICommandRepository<Warehouse> warehouseRepository,
        ICommandRepository<Product> productRepository,
        ICommandRepository<SalesOrder> salesOrderRepository,
        InventoryTransactionService inventoryTransactionService,
        NumberSequenceService numberSequenceService
        )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _salesOrderService = salesOrderService;
        _deliveryOrderRepository = deliveryOrderRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _salesOrderRepository = salesOrderRepository;
        _inventoryTransactionService = inventoryTransactionService;
        _numberSequenceService = numberSequenceService;
    }

    public async Task<CreateSalesOrderItemResult> Handle(CreateSalesOrderItemRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new SalesOrderItem();
        entity.CreatedById = request.CreatedById;

        entity.SalesOrderId = request.SalesOrderId;
        entity.ProductId = request.ProductId;
        entity.Summary = request.Summary;
        entity.UnitPrice = request.UnitPrice;
        entity.Quantity = request.Quantity;

        entity.Total = entity.Quantity * entity.UnitPrice;

        await _repository.CreateAsync(entity, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _salesOrderService.Recalculate(entity.SalesOrderId ?? "");

        // Load the product to check if it's physical
        var product = await _productRepository.GetAsync(entity.ProductId ?? string.Empty, cancellationToken);
        if (product == null || !product.Physical.GetValueOrDefault())
        {
            return new CreateSalesOrderItemResult
            {
                Data = entity
            };
        }

        // Check if a DeliveryOrder already exists for this SalesOrder
        var deliveryOrder = await _deliveryOrderRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .FirstOrDefaultAsync(x => x.SalesOrderId == entity.SalesOrderId, cancellationToken);

        if (deliveryOrder == null)
        {
            // Load the SalesOrder for data
            var salesOrder = await _salesOrderRepository.GetAsync(entity.SalesOrderId ?? string.Empty, cancellationToken);
            if (salesOrder == null)
            {
                throw new Exception($"SalesOrder not found: {entity.SalesOrderId}");
            }

            deliveryOrder = new DeliveryOrder();
            deliveryOrder.CreatedById = entity.CreatedById;

            deliveryOrder.Number = _numberSequenceService.GenerateNumber(nameof(DeliveryOrder), "", "DO");
            deliveryOrder.DeliveryDate = salesOrder.OrderDate; // Use SalesOrder OrderDate
            deliveryOrder.Status = DeliveryOrderStatus.Confirmed; // Assuming Confirmed as default
            deliveryOrder.Description = salesOrder.Description;
            deliveryOrder.SalesOrderId = entity.SalesOrderId;

            await _deliveryOrderRepository.CreateAsync(deliveryOrder, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        // Get the first non-system warehouse
        var defaultWarehouse = await _warehouseRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .Where(x => (bool)!x.SystemWarehouse)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultWarehouse != null)
        {
            await _inventoryTransactionService.DeliveryOrderCreateInvenTrans(
                deliveryOrder.Id,
                defaultWarehouse.Id,
                entity.ProductId,
                entity.Quantity,
                entity.CreatedById,
                cancellationToken
            );
        }
        return new CreateSalesOrderItemResult
        {
            Data = entity
        };
    }
}
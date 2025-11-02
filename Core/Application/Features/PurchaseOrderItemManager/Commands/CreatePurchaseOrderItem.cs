using Application.Common.Extensions;
using Application.Common.Repositories;
using Application.Features.InventoryTransactionManager;
using Application.Features.NumberSequenceManager;
using Application.Features.PurchaseOrderManager;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.PurchaseOrderItemManager.Commands;

public class CreatePurchaseOrderItemResult
{
    public PurchaseOrderItem? Data { get; set; }
}

public class CreatePurchaseOrderItemRequest : IRequest<CreatePurchaseOrderItemResult>
{
    public string? PurchaseOrderId { get; init; }
    public string? ProductId { get; init; }
    public string? Summary { get; init; }
    public double? UnitPrice { get; init; }
    public double? Quantity { get; init; }
    public string? CreatedById { get; init; }
}

public class CreatePurchaseOrderItemValidator : AbstractValidator<CreatePurchaseOrderItemRequest>
{
    public CreatePurchaseOrderItemValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UnitPrice).NotEmpty();
        RuleFor(x => x.Quantity).NotEmpty();
    }
}

public class CreatePurchaseOrderItemHandler : IRequestHandler<CreatePurchaseOrderItemRequest, CreatePurchaseOrderItemResult>
{
    private readonly ICommandRepository<PurchaseOrderItem> _repository;
    private readonly ICommandRepository<GoodsReceive> _goodsReceiveRepository;
    private readonly ICommandRepository<Warehouse> _warehouseRepository;
    private readonly ICommandRepository<Product> _productRepository;
    private readonly ICommandRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PurchaseOrderService _purchaseOrderService;
    private readonly InventoryTransactionService _inventoryTransactionService;
    private readonly NumberSequenceService _numberSequenceService;

    public CreatePurchaseOrderItemHandler(
        ICommandRepository<PurchaseOrderItem> repository,
        ICommandRepository<GoodsReceive> goodsReceiveRepository,
        ICommandRepository<Warehouse> warehouseRepository,
        ICommandRepository<Product> productRepository,
        ICommandRepository<PurchaseOrder> purchaseOrderRepository,
        IUnitOfWork unitOfWork,
        PurchaseOrderService purchaseOrderService,
        InventoryTransactionService inventoryTransactionService,
        NumberSequenceService numberSequenceService
    )
    {
        _repository = repository;
        _goodsReceiveRepository = goodsReceiveRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
        _purchaseOrderService = purchaseOrderService;
        _inventoryTransactionService = inventoryTransactionService;
        _numberSequenceService = numberSequenceService;
    }

    public async Task<CreatePurchaseOrderItemResult> Handle(CreatePurchaseOrderItemRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new PurchaseOrderItem();
        entity.CreatedById = request.CreatedById;

        entity.PurchaseOrderId = request.PurchaseOrderId;
        entity.ProductId = request.ProductId;
        entity.Summary = request.Summary;
        entity.UnitPrice = request.UnitPrice;
        entity.Quantity = request.Quantity;

        entity.Total = entity.Quantity * entity.UnitPrice;

        await _repository.CreateAsync(entity, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        _purchaseOrderService.Recalculate(entity.PurchaseOrderId ?? "");

        // Load the product to check if it's physical
        var product = await _productRepository.GetAsync(entity.ProductId ?? string.Empty, cancellationToken);
        if (product == null || !product.Physical.GetValueOrDefault())
        {
            return new CreatePurchaseOrderItemResult
            {
                Data = entity
            };
        }

        // Check if a GoodsReceive already exists for this PurchaseOrder
        var goodsReceive = await _goodsReceiveRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == entity.PurchaseOrderId, cancellationToken);

        if (goodsReceive == null)
        {
            // Load the PurchaseOrder for data
            var purchaseOrder = await _purchaseOrderRepository.GetAsync(
                entity.PurchaseOrderId ?? string.Empty,
                cancellationToken
            );

            if (purchaseOrder == null)
            {
                throw new Exception($"PurchaseOrder not found: {entity.PurchaseOrderId}");
            }

            goodsReceive = new GoodsReceive
            {
                CreatedById = entity.CreatedById,
                Number = _numberSequenceService.GenerateNumber(nameof(GoodsReceive), "", "GR"),
                ReceiveDate = purchaseOrder.OrderDate,
                Status = GoodsReceiveStatus.Confirmed,
                Description = purchaseOrder.Description,
                PurchaseOrderId = entity.PurchaseOrderId
            };

            await _goodsReceiveRepository.CreateAsync(goodsReceive, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        // Get the first non-system warehouse
        var defaultWarehouse = await _warehouseRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .Where(x => x.SystemWarehouse == false)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultWarehouse != null)
        {
            await _inventoryTransactionService.GoodsReceiveCreateInvenTrans(
                goodsReceive.Id,
                defaultWarehouse.Id,
                entity.ProductId,
                entity.Quantity,
                entity.CreatedById,
                cancellationToken
            );
        }

        return new CreatePurchaseOrderItemResult
        {
            Data = entity
        };
    }
    private async Task<bool> ProcessGoodsReceive(
        PurchaseOrderItem item,
        DateTime receiveDate,
        string? userId,
        CancellationToken cancellationToken)
    {
        // Get product to check if it's physical
        var product = await _productRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .Where(x => x.Id == item.ProductId)
            .FirstOrDefaultAsync(cancellationToken);

        if (product?.Physical != true)
        {
            return false; // Non-physical products don't create inventory transactions
        }

        // Get default warehouse
        var defaultWarehouse = await _warehouseRepository
            .GetQuery()
            .ApplyIsDeletedFilter(false)
            .Where(x => x.SystemWarehouse == false)
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultWarehouse == null)
        {
            throw new Exception("No default warehouse found. Please configure a warehouse first.");
        }

        // Create inventory transaction
        await _inventoryTransactionService.GoodsReceiveCreateInvenTrans(
            item.PurchaseOrderId ?? "",
            defaultWarehouse.Id,
            item.ProductId,
            item.Quantity,
            userId,
            cancellationToken
        );

        return true;
    }
}
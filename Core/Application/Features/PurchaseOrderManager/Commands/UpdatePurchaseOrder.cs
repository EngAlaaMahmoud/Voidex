using Application.Common.Extensions;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.PurchaseOrderManager.Commands;

public class UpdatePurchaseOrderResult
{
    public PurchaseOrder? Data { get; set; }
}

public class UpdatePurchaseOrderRequest : IRequest<UpdatePurchaseOrderResult>
{
    public string? Id { get; init; }
    //public string? OrderStatus { get; init; }
    public string? Description { get; init; }
    public string? VendorId { get; init; }
    public string? UpdatedById { get; init; }
    public string? TaxId { get; init; }      // optional
    public double? Discount { get; init; }
}

public class UpdatePurchaseOrderValidator : AbstractValidator<UpdatePurchaseOrderRequest>
{
    public UpdatePurchaseOrderValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        //RuleFor(x => x.OrderStatus).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
    }
}

public class UpdatePurchaseOrderHandler : IRequestHandler<UpdatePurchaseOrderRequest, UpdatePurchaseOrderResult>
{
    private readonly ICommandRepository<PurchaseOrder> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PurchaseOrderService _purchaseOrderService;
    private readonly ICommandRepository<Tax> _taxRepository;

    public UpdatePurchaseOrderHandler(
        ICommandRepository<PurchaseOrder> repository,
        IUnitOfWork unitOfWork,
        PurchaseOrderService purchaseOrderService,
        ICommandRepository<Tax> taxRepository
        )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _purchaseOrderService = purchaseOrderService;
        _taxRepository = taxRepository;
    }

    public async Task<UpdatePurchaseOrderResult> Handle(UpdatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var id = request.Id ?? string.Empty;

        // Load PO without taxes since we're removing tax functionality
        var entity = await _repository
            .GetQuery()
            .ApplyIsDeletedFilter()
            .Include(x => x.Tax)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entity == null)
            throw new Exception($"Entity not found: {request.Id}");

        // update simple properties
        entity.UpdatedById = request.UpdatedById;
        entity.OrderStatus = PurchaseOrderStatus.Confirmed;
        entity.Description = request.Description;
        entity.VendorId = request.VendorId;
        entity.Discount = request.Discount ?? 0;

        // ----- TaxId handling (same as SalesOrder) -----
        if (string.IsNullOrEmpty(request.TaxId))
        {
            entity.TaxId = null;
        }
        else
        {
            var taxExists = await _taxRepository.GetQuery()
                .AnyAsync(t => t.Id == request.TaxId, cancellationToken);
            if (!taxExists)
                throw new Exception($"Invalid TaxId: {request.TaxId}");
            entity.TaxId = request.TaxId;
        }

        // persist changes
        _repository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        // recalc totals (now based on product-level taxes)
        _purchaseOrderService.Recalculate(entity.Id);

        return new UpdatePurchaseOrderResult
        {
            Data = entity
        };
    }
}


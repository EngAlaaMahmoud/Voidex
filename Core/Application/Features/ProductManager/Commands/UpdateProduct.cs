using Application.Common.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.ProductManager.Commands;

public class UpdateProductResult
{
    public Product? Data { get; set; }
}

public class UpdateProductRequest : IRequest<UpdateProductResult>
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Barcode { get; set; } // New
    public string? Description { get; init; }
    public double? UnitPrice { get; init; }
    public bool? Physical { get; init; } = true;
    public string? UnitMeasureId { get; init; }
    public string? ProductGroupId { get; init; }
    public string? UpdatedById { get; init; }
    public string? VatId { get; init; }        // Added VAT
    //public string? TaxId { get; init; }        // Added Tax
    // new fields
    public string? InternalCode { get; init; }
    public string? GisEgsCode { get; init; }
    public string? CompanyName { get; init; }
    public string? Model { get; init; }
    public double? Discount { get; init; }
    public double? PriceAfterDiscount { get; init; }
    public double? ServiceFee { get; init; }
    public double? AdditionalTax { get; init; }
    public double? AdditionalFee { get; init; }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.UnitPrice).NotEmpty();
        RuleFor(x => x.Physical).NotEmpty();
        RuleFor(x => x.UnitMeasureId).NotEmpty();
        RuleFor(x => x.ProductGroupId).NotEmpty();
        RuleFor(x => x.VatId).NotEmpty();      // Added VAT validation
        //RuleFor(x => x.TaxId).NotEmpty();      // Added Tax validation
    }
}

public class UpdateProductHandler : IRequestHandler<UpdateProductRequest, UpdateProductResult>
{
    private readonly ICommandRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductHandler(
        ICommandRepository<Product> repository,
        IUnitOfWork unitOfWork
        )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateProductResult> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {

        var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);

        if (entity == null)
        {
            throw new Exception($"Entity not found: {request.Id}");
        }

        entity.UpdatedById = request.UpdatedById;

        entity.Name = request.Name;
        entity.UnitPrice = request.UnitPrice;
        entity.Physical = request.Physical;
        entity.Description = request.Description;
        entity.UnitMeasureId = request.UnitMeasureId;
        entity.ProductGroupId = request.ProductGroupId;
        entity.VatId = request.VatId;          // Added VAT
        //entity.TaxId = request.TaxId;          // Added 
        entity.Barcode = request.Barcode;
        entity.InternalCode = request.InternalCode;
        entity.GisEgsCode = request.GisEgsCode;
        entity.CompanyName = request.CompanyName;
        entity.Model = request.Model;
        entity.Discount = request.Discount;
        entity.PriceAfterDiscount = request.PriceAfterDiscount;
        entity.ServiceFee = request.ServiceFee;
        entity.AdditionalTax = request.AdditionalTax;
        entity.AdditionalFee = request.AdditionalFee;
        _repository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new UpdateProductResult
        {
            Data = entity
        };
    }
}


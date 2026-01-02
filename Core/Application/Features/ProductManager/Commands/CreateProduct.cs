using Application.Common.Repositories;
using Application.Features.NumberSequenceManager;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductManager.Commands;

public class CreateProductResult
{
    public Product? Data { get; set; }
}

public class CreateProductRequest : IRequest<CreateProductResult>
{
    public string? Number { get; init; }
    public string? Name { get; init; }
    public string Barcode { get; set; } // New
    public string? Description { get; init; }
    public double? UnitPrice { get; init; }
    public bool? Physical { get; init; } = true;
    public string? UnitMeasureId { get; init; }
    public string? ProductGroupId { get; init; }
    public string? CreatedById { get; init; }
    public string? VatId { get; init; }        // Added VAT
    public string? TaxId { get; init; }        // Added Tax
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

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.UnitPrice).NotEmpty();
        RuleFor(x => x.Physical).NotEmpty();
        RuleFor(x => x.UnitMeasureId).NotEmpty();
        RuleFor(x => x.ProductGroupId).NotEmpty();
        RuleFor(x => x.VatId).NotEmpty();      // Added VAT validation
        RuleFor(x => x.TaxId).NotEmpty();      // Added Tax validation
    }
}

public class CreateProductHandler : IRequestHandler<CreateProductRequest, CreateProductResult>
{
    private readonly ICommandRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NumberSequenceService _numberSequenceService;

    public CreateProductHandler(
        ICommandRepository<Product> repository,
        IUnitOfWork unitOfWork,
        NumberSequenceService numberSequenceService
        )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _numberSequenceService = numberSequenceService;
    }

    public async Task<CreateProductResult> Handle(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Product();
        var existingProduct = await _repository.GetQuery().Where(a => a.Barcode == request.Barcode)
           .AsNoTracking()
           .AnyAsync(p => p.Number == request.Number && !p.IsDeleted, cancellationToken);
        if (existingProduct)
        {
            throw new InvalidOperationException("Barcode already exists");
        }
        entity.CreatedById = request.CreatedById;

        entity.Number = _numberSequenceService.GenerateNumber(nameof(Product), "", "ART");
        entity.Name = request.Name;
        entity.UnitPrice = request.UnitPrice;
        entity.Physical = request.Physical;
        entity.Description = request.Description;
        entity.UnitMeasureId = request.UnitMeasureId;
        entity.ProductGroupId = request.ProductGroupId;
        entity.VatId = request.VatId;          // Added VAT
        //entity.TaxId = request.TaxId;          // Added  Taxes
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

        await _repository.CreateAsync(entity, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new CreateProductResult
        {
            Data = entity
        };
    }
}
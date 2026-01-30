using Application.Common.Repositories;
using Application.Features.NumberSequenceManager;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductManager.Commands;


public class ProductTaxDto
{
    public string? TaxId { get; set; }
    public double? TaxValue { get; set; }
    public double? Percentage { get; set; }
    public string? MainCode { get; set; }
    public string? SubCode { get; set; }
}
public class CreateProductResult
{
    public Product? Data { get; set; }
}


public class CreateProductRequest : IRequest<CreateProductResult>
{
    public string? Number { get; init; }
    public string? Name { get; init; }
    public string? Barcode { get; set; }
    public string? Description { get; init; }
    public double? UnitPrice { get; init; }
    public bool? Physical { get; init; } = true;
    public string? UnitMeasureId { get; init; }
    public string? ProductGroupId { get; init; }
    public string? ProductCompanyId { get; init; }
    public string? CreatedById { get; init; }
    public string? VatId { get; init; }

    // Removed single TaxId
    // public string? TaxId { get; init; }

    // New fields
    public string? InternalCode { get; init; }
    public string? GisEgsCode { get; init; }
    public string? CompanyName { get; init; }
    public string? Model { get; init; }
    public double? Discount { get; init; }
    public double? PriceAfterDiscount { get; init; }
    public double? ServiceFee { get; init; }
    public double? AdditionalTax { get; init; }
    public double? AdditionalFee { get; init; }

    // Add list of product taxes
    public List<ProductTaxDto> ProductTaxes { get; init; } = new();
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
        // Removed TaxId validation
        // RuleFor(x => x.TaxId).NotEmpty();
    }
}

public class CreateProductHandler : IRequestHandler<CreateProductRequest, CreateProductResult>
{
    private readonly ICommandRepository<Product> _repository;
    private readonly ICommandRepository<ProductTax> _productTaxRepository;
    private readonly ICommandRepository<Tax> _taxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NumberSequenceService _numberSequenceService;

    public CreateProductHandler(
        ICommandRepository<Product> repository,
        ICommandRepository<ProductTax> productTaxRepository,
        ICommandRepository<Tax> taxRepository,
        IUnitOfWork unitOfWork,
        NumberSequenceService numberSequenceService)
    {
        _repository = repository;
        _productTaxRepository = productTaxRepository;
        _taxRepository = taxRepository;
        _unitOfWork = unitOfWork;
        _numberSequenceService = numberSequenceService;
    }

    public async Task<CreateProductResult> Handle(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        // Check if barcode already exists
        var existingProduct = await _repository.GetQuery()
            .Where(a => a.Barcode == request.Barcode && !a.IsDeleted)
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (existingProduct)
        {
            throw new InvalidOperationException("Barcode already exists");
        }

        // Create product entity
        var entity = new Product
        {
            CreatedById = request.CreatedById,
            Number = _numberSequenceService.GenerateNumber(nameof(Product), "", "ART"),
            Name = request.Name,
            UnitPrice = request.UnitPrice ?? 0,
            Physical = request.Physical ?? true,
            Description = request.Description,
            UnitMeasureId = request.UnitMeasureId,
            ProductGroupId = request.ProductGroupId,
            ProductCompanyId = request.ProductCompanyId,
            VatId = request.VatId,
            Barcode = request.Barcode,
            InternalCode = request.InternalCode,
            GisEgsCode = request.GisEgsCode,
            CompanyName = request.CompanyName,
            Model = request.Model,
            Discount = request.Discount,
            PriceAfterDiscount = request.PriceAfterDiscount,
            ServiceFee = request.ServiceFee,
            AdditionalTax = request.AdditionalTax,
            AdditionalFee = request.AdditionalFee
        };

        await _repository.CreateAsync(entity, cancellationToken);

        // Save product first to get the ID
        await _unitOfWork.SaveAsync(cancellationToken);

        // Add product taxes if any
        if (request.ProductTaxes != null && request.ProductTaxes.Any())
        {
            foreach (var taxDto in request.ProductTaxes)
            {
                if (!string.IsNullOrEmpty(taxDto.TaxId))
                {
                    // Get tax details if needed
                    var tax = await _taxRepository.GetAsync(taxDto.TaxId, cancellationToken);

                    var productTax = new ProductTax
                    {
                        ProductId = entity.Id,
                        TaxId = taxDto.TaxId,
                        TaxValue = taxDto.TaxValue ?? 0,
                        Percentage = taxDto.Percentage ?? (tax?.Percentage ?? 0),
                        MainCode = taxDto.MainCode ?? tax?.MainCode ?? "",
                        SubCode = taxDto.SubCode ?? tax?.SubCode ?? ""
                    };

                    await _productTaxRepository.CreateAsync(productTax, cancellationToken);
                }
            }

            // Save product taxes
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        return new CreateProductResult
        {
            Data = entity
        };
    }
}
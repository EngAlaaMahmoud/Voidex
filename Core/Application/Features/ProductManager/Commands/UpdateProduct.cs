using Application.Common.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductManager.Commands;

public class UpdateProductResult
{
    public Product? Data { get; set; }
}

public class UpdateProductRequest : IRequest<UpdateProductResult>
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Barcode { get; set; }
    public string? Description { get; init; }
    public double? UnitPrice { get; init; }
    public bool? Physical { get; init; } = true;
    public string? UnitMeasureId { get; init; }
    public string? ProductGroupId { get; init; }
    public string? ProductCompanyId { get; init; }
    public string? UpdatedById { get; init; }
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
        RuleFor(x => x.VatId).NotEmpty();
        // Removed TaxId validation
        // RuleFor(x => x.TaxId).NotEmpty();
    }
}

public class UpdateProductHandler : IRequestHandler<UpdateProductRequest, UpdateProductResult>
{
    private readonly ICommandRepository<Product> _repository;
    private readonly ICommandRepository<ProductTax> _productTaxRepository;
    private readonly ICommandRepository<Tax> _taxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductHandler(
        ICommandRepository<Product> repository,
        ICommandRepository<ProductTax> productTaxRepository,
        ICommandRepository<Tax> taxRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _productTaxRepository = productTaxRepository;
        _taxRepository = taxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateProductResult> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        // Load product with its existing taxes
        var entity = await _repository.GetQuery()
            .Include(p => p.ProductTaxes)
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new Exception($"Entity not found: {request.Id}");
        }

        // Update product properties
        entity.UpdatedById = request.UpdatedById;
        entity.Name = request.Name;
        entity.UnitPrice = request.UnitPrice ?? 0;
        entity.Physical = request.Physical ?? true;
        entity.Description = request.Description;
        entity.UnitMeasureId = request.UnitMeasureId;
        entity.ProductGroupId = request.ProductGroupId;
        entity.ProductCompanyId = request.ProductCompanyId;
        entity.VatId = request.VatId;
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

        // Remove existing product taxes
        if (entity.ProductTaxes.Any())
        {
            foreach (var productTax in entity.ProductTaxes.ToList())
            {
                _productTaxRepository.Delete(productTax);
            }
        }

        // Add new product taxes
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
        }

        await _unitOfWork.SaveAsync(cancellationToken);

        return new UpdateProductResult
        {
            Data = entity
        };
    }
}
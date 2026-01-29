using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductManager.Queries;

public record ProductTaxDto
{
    public string? TaxId { get; init; }
    public string? MainCode { get; init; }
    public string? SubCode { get; init; }
    public string? TaxCategoryName { get; init; }
    public string? Description { get; init; }
    public double? Percentage { get; init; }
    public double? TaxValue { get; init; }
}
public record GetProductListDto
{
    public string? Id { get; init; }
    public string? Number { get; init; }
    public string? Name { get; init; }
    public string? Barcode { get; set; }
    public string? Description { get; init; }
    public double? UnitPrice { get; init; }
    public bool? Physical { get; init; }
    public string? UnitMeasureId { get; init; }
    public string? UnitMeasureName { get; init; }
    public string? ProductGroupId { get; init; }
    public string? ProductGroupName { get; init; }
    public string? ProductCompanyId { get; init; }
    public string? ProductCompanyName { get; init; }
    public DateTime? CreatedAtUtc { get; init; }

    // New fields
    public string? InternalCode { get; init; }
    public string? GisEgsCode { get; init; }
    public string? CompanyName { get; init; }
    public string? Model { get; init; }
    public double? Discount { get; init; }
    public double? PriceAfterDiscount { get; init; }

    // Tax fields based on categories
    public double? ServiceFee { get; init; }
    public double? AdditionalTax { get; init; }
    public double? AdditionalFee { get; init; }
    public double? OtherTax1 { get; init; }
    public double? OtherTax2 { get; init; }

    // List of all taxes for reference
    public Dictionary<string, double> TaxCategories { get; init; } = new();

    public List<ProductTaxDto> ProductTaxes { get; init; } = new();

    // Summary
    public double? TotalTaxes { get; init; }
    public double? FinalPrice { get; init; }
}
public class GetProductListProfile : Profile
{
    public GetProductListProfile()
    {
        CreateMap<Product, GetProductListDto>()
            .ForMember(
                dest => dest.UnitMeasureName,
                opt => opt.MapFrom(src => src.UnitMeasure != null ? src.UnitMeasure.Name : string.Empty)
            )
            .ForMember(
                dest => dest.ProductGroupName,
                opt => opt.MapFrom(src => src.ProductGroup != null ? src.ProductGroup.Name : string.Empty)
            )
            .ForMember(
                dest => dest.ProductCompanyName,
                opt => opt.MapFrom(src => src.ProductCompany != null ? src.ProductCompany.Name : string.Empty)
            )
            .ForMember(dest => dest.ProductTaxes, opt => opt.MapFrom(src => src.ProductTaxes.Select(pt =>
                new ProductTaxDto
                {
                    TaxId = pt.TaxId,
                    MainCode = pt.MainCode,
                    SubCode = pt.SubCode,
                    TaxCategoryName = pt.Tax != null && pt.Tax.TaxCategory != null ?
                        pt.Tax.TaxCategory.NameAr : string.Empty,
                    Description = pt.Tax != null ? pt.Tax.Description : string.Empty,
                    Percentage = pt.Percentage,
                    TaxValue = pt.TaxValue
                })))
            // Group taxes by category and map to specific properties
            .ForMember(dest => dest.ServiceFee, opt => opt.MapFrom(src =>
                src.ProductTaxes
                    .Where(pt => pt.Tax != null && pt.Tax.TaxCategory != null &&
                        (pt.Tax.TaxCategory.NameAr.Contains("رسوم الخدمة") ||
                         pt.Tax.TaxCategory.NameAr.Contains("Service Fee")))
                    .Sum(pt => pt.TaxValue)))
            .ForMember(dest => dest.AdditionalTax, opt => opt.MapFrom(src =>
                src.ProductTaxes
                    .Where(pt => pt.Tax != null && pt.Tax.TaxCategory != null &&
                        (pt.Tax.TaxCategory.NameAr.Contains("ضريبة إضافية") ||
                         pt.Tax.TaxCategory.NameAr.Contains("Additional Tax")))
                    .Sum(pt => pt.TaxValue)))
            .ForMember(dest => dest.AdditionalFee, opt => opt.MapFrom(src =>
                src.ProductTaxes
                    .Where(pt => pt.Tax != null && pt.Tax.TaxCategory != null &&
                        (pt.Tax.TaxCategory.NameAr.Contains("رسوم إضافية") ||
                         pt.Tax.TaxCategory.NameAr.Contains("Additional Fee")))
                    .Sum(pt => pt.TaxValue)))
            .ForMember(dest => dest.TotalTaxes, opt => opt.MapFrom(src =>
                src.ProductTaxes.Sum(pt => pt.TaxValue)))
            .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src =>
                (src.PriceAfterDiscount ?? src.UnitPrice ?? 0) +
                (src.ProductTaxes.Sum(pt => pt.TaxValue))))
             .ForMember(dest => dest.TaxCategories, opt => opt.MapFrom(src =>
                src.ProductTaxes
                    .GroupBy(pt => pt.Tax != null && pt.Tax.TaxCategory != null ?
                        pt.Tax.TaxCategory.NameAr : "Uncategorized")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(pt => pt.TaxValue)
                    )))
            .ForMember(dest => dest.TotalTaxes, opt => opt.MapFrom(src =>
                src.ProductTaxes.Sum(pt => pt.TaxValue)))
            .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src =>
                (src.PriceAfterDiscount ?? src.UnitPrice ?? 0) +
                src.ProductTaxes.Sum(pt => pt.TaxValue)));
    }
}

public class GetProductListResult
{
    public List<GetProductListDto>? Data { get; init; }
}

public class GetProductListRequest : IRequest<GetProductListResult>
{
    public bool IsDeleted { get; init; } = false;
}

public class GetProductListHandler : IRequestHandler<GetProductListRequest, GetProductListResult>
{
    private readonly IMapper _mapper;
    private readonly IQueryContext _context;

    public GetProductListHandler(IMapper mapper, IQueryContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<GetProductListResult> Handle(GetProductListRequest request, CancellationToken cancellationToken)
    {
        var query = _context.Product
            .AsNoTracking()
            .ApplyIsDeletedFilter(request.IsDeleted)
            .Include(x => x.UnitMeasure)
            .Include(x => x.ProductGroup)
            .Include(x => x.ProductCompany)
            .Include(x => x.ProductTaxes)
                .ThenInclude(pt => pt.Tax)
                    .ThenInclude(t => t.TaxCategory)
            .AsQueryable();

        var entities = await query.ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<GetProductListDto>>(entities);

        return new GetProductListResult
        {
            Data = dtos
        };
    }
}

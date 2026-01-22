using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductManager.Queries;

public record GetProductListDto
{
    public string? Id { get; init; }
    public string? Number { get; init; }
    public string? Name { get; init; }
    public string Barcode { get; set; } // New
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
    public string? VatId { get; init; }
    public string? VatName { get; init; }
    public double? VatPercentage { get; init; }  // Add this
    public string? TaxId { get; init; }
    public string? TaxName { get; init; }
    public double? TaxPercentage { get; init; }  // Add this
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
            .ForMember(
                dest => dest.VatName,
                opt => opt.MapFrom(src => src.Vat != null ? src.Vat.Name : "Not Set")
            )
            .ForMember(
                dest => dest.VatPercentage,
                opt => opt.MapFrom(src => src.Vat != null ? src.Vat.Percentage : 0.0)
            );
            //.ForMember(
            //    dest => dest.TaxName,
            //    opt => opt.MapFrom(src => src.Tax != null ? src.Tax.Name : "Not Set")
            //)
            //.ForMember(
            //    dest => dest.TaxPercentage,
            //    opt => opt.MapFrom(src => src.Tax != null ? src.Tax.Percentage : 0.0)
            //);
    }
    //public GetProductListProfile()
    //{
    //    CreateMap<Product, GetProductListDto>()
    //        .ForMember(
    //            dest => dest.UnitMeasureName,
    //            opt => opt.MapFrom(src => src.UnitMeasure != null ? src.UnitMeasure.Name : string.Empty)
    //        )
    //        .ForMember(
    //            dest => dest.ProductGroupName,
    //            opt => opt.MapFrom(src => src.ProductGroup != null ? src.ProductGroup.Name : string.Empty)
    //        );

    //}
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
        var query = _context
            .Product
            .AsNoTracking()
            .ApplyIsDeletedFilter(request.IsDeleted)
            .Include(x => x.UnitMeasure)
            .Include(x => x.ProductGroup)
            .Include(x => x.Vat)
            //.Include(x => x.Tax)
            .AsQueryable();

        var entities = await query.ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<GetProductListDto>>(entities);

        return new GetProductListResult
        {
            Data = dtos
        };
    }


}




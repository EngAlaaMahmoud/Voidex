using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.ProductManager.Queries
{
    public record GetProductByBarcodeDto
    {
        public string? Id { get; init; }
        public string? Number { get; init; }
        public string? Name { get; init; }
        public string? Barcode { get; init; }
        public string? Description { get; init; }
        public double? UnitPrice { get; init; }
        public bool? Physical { get; init; }
        public string? UnitMeasureId { get; init; }
        public string? UnitMeasureName { get; init; }
        public string? ProductGroupId { get; init; }
        public string? ProductGroupName { get; init; }
        public DateTime? CreatedAtUtc { get; init; }
        public string? VatId { get; init; }
        public string? VatName { get; init; }
        public double? VatPercentage { get; init; }
        public string? TaxId { get; init; }
        public string? TaxName { get; init; }
        public double? TaxPercentage { get; init; }
    }

    public class GetProductByBarcodeProfile : Profile
    {
        public GetProductByBarcodeProfile()
        {
            CreateMap<Product, GetProductByBarcodeDto>()
                .ForMember(
                    dest => dest.UnitMeasureName,
                    opt => opt.MapFrom(src => src.UnitMeasure != null ? src.UnitMeasure.Name : string.Empty)
                )
                .ForMember(
                    dest => dest.ProductGroupName,
                    opt => opt.MapFrom(src => src.ProductGroup != null ? src.ProductGroup.Name : string.Empty)
                )
                .ForMember(
                    dest => dest.VatName,
                    opt => opt.MapFrom(src => src.Vat != null ? src.Vat.Name : "Not Set")
                )
                .ForMember(
                    dest => dest.VatPercentage,
                    opt => opt.MapFrom(src => src.Vat != null ? src.Vat.Percentage : 0.0)
                );
                
        }
    }
    public class GetProductByBarcodeResult
    {
        public GetProductByBarcodeDto? Data { get; init; }
    }

    public class GetProductByBarcodeRequest : IRequest<GetProductByBarcodeResult>
    {
        public string Barcode { get; init; } = string.Empty;
        public bool IsDeleted { get; init; } = false;
    }
    public class GetProductByBarcodeHandler : IRequestHandler<GetProductByBarcodeRequest, GetProductByBarcodeResult>
    {
        private readonly IMapper _mapper;
        private readonly IQueryContext _context;

        public GetProductByBarcodeHandler(IMapper mapper, IQueryContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<GetProductByBarcodeResult> Handle(GetProductByBarcodeRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Barcode))
            {
                return new GetProductByBarcodeResult { Data = null };
            }

            var product = await _context
                .Product
                .AsNoTracking()
                .ApplyIsDeletedFilter(request.IsDeleted)
                .Include(x => x.UnitMeasure)
                .Include(x => x.ProductGroup)
                .Include(x => x.Vat)
                //.Include(x => x.Tax)
                .Where(p => p.Barcode == request.Barcode)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return new GetProductByBarcodeResult { Data = null };
            }

            var dto = _mapper.Map<GetProductByBarcodeDto>(product);

            return new GetProductByBarcodeResult
            {
                Data = dto
            };
        }
    }

}

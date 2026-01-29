using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TaxManager.Queries;

public record GetTaxListDto
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public double? Percentage { get; init; }
    public string? Description { get; init; }

    // New fields for tax register
    public string? MainCode { get; init; }
    public string? TypeCode { get; init; }
    public string? SubCode { get; init; }
    public string? TaxType { get; init; }

    public DateTime? CreatedAtUtc { get; init; }
    public string? TaxCategoryId { get; init; }
    public string? TaxCategoryName { get; init; }  // ← add this
    // NEW - category fields
    public string? TaxCategoryCode { get; init; }
}

public class GetTaxListProfile : Profile
{
    public GetTaxListProfile()
    {
        CreateMap<Tax, GetTaxListDto>()
            .ForMember(dest => dest.TaxCategoryId, opt => opt.MapFrom(src => src.TaxCategory != null ? src.TaxCategory.Id : null))
            .ForMember(dest => dest.TaxCategoryCode, opt => opt.MapFrom(src => src.TaxCategory != null ? src.TaxCategory.Code : null))
            .ForMember(dest => dest.TaxCategoryName, opt => opt.MapFrom(src => src.TaxCategory != null ? src.TaxCategory.NameAr : null)); ;
    }
}

public class GetTaxListResult
{
    public List<GetTaxListDto>? Data { get; init; }
}

public class GetTaxListRequest : IRequest<GetTaxListResult>
{
    public bool IsDeleted { get; init; } = false;
}


public class GetTaxListHandler : IRequestHandler<GetTaxListRequest, GetTaxListResult>
{
    private readonly IMapper _mapper;
    private readonly IQueryContext _context;

    public GetTaxListHandler(IMapper mapper, IQueryContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<GetTaxListResult> Handle(GetTaxListRequest request, CancellationToken cancellationToken)
    {
        var query = _context
            .Tax
            .AsNoTracking()
            .Include(t => t.TaxCategory)
            .ApplyIsDeletedFilter(request.IsDeleted)
            .AsQueryable();

        var entities = await query.ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<GetTaxListDto>>(entities);

        return new GetTaxListResult
        {
            Data = dtos
        };
    }


}




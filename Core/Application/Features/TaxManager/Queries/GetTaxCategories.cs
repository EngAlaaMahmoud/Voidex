using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.TaxManager.Queries;

public record GetTaxCategoriesDto
{
    public string Id { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? NameEn { get; init; }
    public int? SortOrder { get; init; }
    public string? Description { get; init; }
    public DateTime? CreatedAtUtc { get; init; }

    // Optional: count of taxes using this category (useful for UI warnings)
    public int TaxCount { get; init; }
}

public record GetTaxCategoriesResult
{
    public List<GetTaxCategoriesDto> Data { get; init; } = new();
}

public class GetTaxCategoriesRequest : IRequest<GetTaxCategoriesResult>
{
    public bool IsDeleted { get; init; } = false;
}

public class GetTaxCategoriesProfile : Profile
{
    public GetTaxCategoriesProfile()
    {
        CreateMap<TaxCategory, GetTaxCategoriesDto>()
            .ForMember(dest => dest.TaxCount,
                opt => opt.MapFrom(src => src.Taxes.Count));
    }
}

public class GetTaxCategoriesHandler : IRequestHandler<GetTaxCategoriesRequest, GetTaxCategoriesResult>
{
    private readonly IMapper _mapper;
    private readonly IQueryContext _context;

    public GetTaxCategoriesHandler(IMapper mapper, IQueryContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<GetTaxCategoriesResult> Handle(
        GetTaxCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var query = _context
            .TaxCategory
            .AsNoTracking()
            .ApplyIsDeletedFilter(request.IsDeleted)
            .Include(tc => tc.Taxes)   // for TaxCount
            .AsQueryable()
            .OrderBy(tc => tc.SortOrder ?? int.MaxValue)
            .ThenBy(tc => tc.NameAr);

        var entities = await query.ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<GetTaxCategoriesDto>>(entities);

        return new GetTaxCategoriesResult
        {
            Data = dtos
        };
    }
}
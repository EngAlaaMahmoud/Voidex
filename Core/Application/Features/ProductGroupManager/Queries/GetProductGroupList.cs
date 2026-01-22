using Application.Common.CQS.Queries;
using System.Linq;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductGroupManager.Queries;

public record GetProductGroupListDto
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<string>? CompanyIds { get; init; }
    public string? CompanyNames { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
}

public class GetProductGroupListProfile : Profile
{
    public GetProductGroupListProfile()
    {
        CreateMap<ProductGroup, GetProductGroupListDto>()
            .ForMember(dest => dest.CompanyIds, opt => opt.MapFrom(src => src.ProductCompanies != null ? src.ProductCompanies.Select(c => c.Id).ToList() : new List<string>()))
            .ForMember(dest => dest.CompanyNames, opt => opt.MapFrom(src => src.ProductCompanies != null ? string.Join(", ", src.ProductCompanies.Select(c => c.Name)) : string.Empty));
    }
}

public class GetProductGroupListResult
{
    public List<GetProductGroupListDto>? Data { get; init; }
}

public class GetProductGroupListRequest : IRequest<GetProductGroupListResult>
{
    public bool IsDeleted { get; init; } = false;
}


public class GetProductGroupListHandler : IRequestHandler<GetProductGroupListRequest, GetProductGroupListResult>
{
    private readonly IMapper _mapper;
    private readonly IQueryContext _context;

    public GetProductGroupListHandler(IMapper mapper, IQueryContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<GetProductGroupListResult> Handle(GetProductGroupListRequest request, CancellationToken cancellationToken)
    {
        var query = _context
            .ProductGroup
            .AsNoTracking()
            .ApplyIsDeletedFilter(request.IsDeleted)
            .Include(x => x.ProductCompanies)
            .AsQueryable();

        var entities = await query.ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<GetProductGroupListDto>>(entities);

        return new GetProductGroupListResult
        {
            Data = dtos
        };
    }


}




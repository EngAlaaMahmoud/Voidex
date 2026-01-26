using Application.Common.CQS.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductCompanyManager.Queries;

public record GetProductCompanyListDto
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Street { get; init; }      // <-- add these
    public string? City { get; init; }
    public string? Description { get; init; }
    public DateTime? CreatedAtUtc { get; init; }
}

public class GetProductCompanyListProfile : Profile
{
    public GetProductCompanyListProfile()
    {
        CreateMap<ProductCompany, GetProductCompanyListDto>();
    }
}

public class GetProductCompanyListResult
{
    public List<GetProductCompanyListDto>? Data { get; init; }
}

public class GetProductCompanyListRequest : IRequest<GetProductCompanyListResult>
{
    public bool IsDeleted { get; init; } = false;
}

public class GetProductCompanyListHandler : IRequestHandler<GetProductCompanyListRequest, GetProductCompanyListResult>
{
    private readonly IMapper _mapper;
    private readonly IQueryContext _context;

    public GetProductCompanyListHandler(IMapper mapper, IQueryContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<GetProductCompanyListResult> Handle(GetProductCompanyListRequest request, CancellationToken cancellationToken)
    {
        var query = _context
            .ProductCompany
            .AsNoTracking()
            .AsQueryable();

        var entities = await query.ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<GetProductCompanyListDto>>(entities);

        return new GetProductCompanyListResult { Data = dtos };
    }
}

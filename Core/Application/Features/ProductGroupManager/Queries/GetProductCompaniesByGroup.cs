using Application.Common.CQS.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductGroupManager.Queries;

public record GetProductCompaniesByGroupDto
{
    public string? Id { get; init; }
    public string? Name { get; init; }
}

public class GetProductCompaniesByGroupResult
{
    public List<GetProductCompaniesByGroupDto>? Data { get; init; }
}

public class GetProductCompaniesByGroupRequest : IRequest<GetProductCompaniesByGroupResult>
{
    public string? GroupId { get; init; }
}

public class GetProductCompaniesByGroupHandler : IRequestHandler<GetProductCompaniesByGroupRequest, GetProductCompaniesByGroupResult>
{
    private readonly IQueryContext _context;
    private readonly IMapper _mapper;

    public GetProductCompaniesByGroupHandler(IQueryContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GetProductCompaniesByGroupResult> Handle(GetProductCompaniesByGroupRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.GroupId)) return new GetProductCompaniesByGroupResult { Data = new List<GetProductCompaniesByGroupDto>() };

        // query product companies that are linked to the group to avoid issues with navigation loading
        var items = await _context.ProductCompany
            .AsNoTracking()
            .Where(pc => pc.ProductGroups.Any(pg => pg.Id == request.GroupId))
            .Select(c => new GetProductCompaniesByGroupDto { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);

        return new GetProductCompaniesByGroupResult { Data = items };
    }
}

using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.VatManger.Queries
{
    public record GetVatListDto
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public double? Percentage { get; init; }
        public string? Description { get; init; }
    }

    public class GetVatListProfile : Profile
    {
        public GetVatListProfile()
        {
            CreateMap<Vat, GetVatListDto>();
        }
    }

    public class GetVatListResult
    {
        public List<GetVatListDto>? Data { get; init; }
    }
    public class GetVatListRequest : IRequest<GetVatListResult>
    {
        public bool IsDeleted { get; init; } = false;
    }

    //public class GetVatListQuery : IRequest<GetVatListResult>
    //{
    //    public bool IsDeleted { get; init; } = false;
    //}

    public class GetVatListHandler : IRequestHandler<GetVatListRequest, GetVatListResult>
    {
        private readonly IMapper _mapper;
        private readonly IQueryContext _context;

        public GetVatListHandler(IMapper mapper, IQueryContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<GetVatListResult> Handle(GetVatListRequest request, CancellationToken cancellationToken)
        {
            var query = _context
                .Vats
                .AsNoTracking()
                .ApplyIsDeletedFilter(request.IsDeleted)
                .AsQueryable();

            var entities = await query.ToListAsync(cancellationToken);
            var dtos = _mapper.Map<List<GetVatListDto>>(entities);

            return new GetVatListResult
            {
                Data = dtos
            };
        }
    }
}

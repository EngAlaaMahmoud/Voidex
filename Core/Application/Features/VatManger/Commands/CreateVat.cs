using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.VatManger.Commands
{
    public record CreateVatRequest : IRequest<CreateVatResult>
    {
        public string? Name { get; init; }
        public double? Percentage { get; init; }
        public string? Description { get; init; }
        public string? CreatedById { get; init; }
    }

    public record CreateVatResult
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public double? Percentage { get; init; }
        public string? Description { get; init; }
    }
    public class CreateVatHandler : IRequestHandler<CreateVatRequest, CreateVatResult>
    {
        private readonly ICommandRepository<Vat> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateVatHandler(ICommandRepository<Vat> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateVatResult> Handle(CreateVatRequest request, CancellationToken cancellationToken)
        {
            var vat = new Vat
            {
                Name = request.Name,
                Percentage = request.Percentage,
                Description = request.Description,
                CreatedById = request.CreatedById
            };

            await _repository.CreateAsync(vat);
            await _unitOfWork.SaveAsync();

            return new CreateVatResult
            {
                Id = vat.Id,
                Name = vat.Name,
                Percentage = vat.Percentage,
                Description = vat.Description
            };
        }
    }
}

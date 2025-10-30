using Application.Common.Repositories;
using Application.Features.TaxManager.Commands;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.VatManger.Commands
{
    public record UpdateVatRequest : IRequest<UpdateVatResult>
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public double? Percentage { get; init; }
        public string? Description { get; init; }
        public string? UpdatedById { get; init; }
    }

    public record UpdateVatResult
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public double? Percentage { get; init; }
        public string? Description { get; init; }
    }
    public class UpdateVatHandler : IRequestHandler<UpdateVatRequest, UpdateVatResult>
    {
        private readonly ICommandRepository<Vat> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateVatHandler(ICommandRepository<Vat> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateVatResult> Handle(UpdateVatRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);

            if (entity == null)
            {
                throw new Exception($"Entity not found: {request.Id}");
            }

            entity.Name = request.Name;
            entity.Percentage = request.Percentage;
            entity.Description = request.Description;
            entity.UpdatedById = request.UpdatedById;

             _repository.Update(entity);
            await _unitOfWork.SaveAsync();

            return new UpdateVatResult
            {
                Id = entity.Id,
                Name = entity.Name,
                Percentage = entity.Percentage,
                Description = entity.Description
            };
        }
    }
}

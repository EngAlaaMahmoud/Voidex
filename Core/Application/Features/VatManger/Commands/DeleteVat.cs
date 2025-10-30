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
    public record DeleteVatRequest : IRequest<DeleteVatResult>
    {
        public string? Id { get; init; }
        public string? DeletedById { get; init; }
    }

    public record DeleteVatResult
    {
        public string? Id { get; init; }
        public bool IsDeleted { get; init; }
    }
    public class DeleteVatHandler : IRequestHandler<DeleteVatRequest, DeleteVatResult>
    {
        private readonly ICommandRepository<Vat> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteVatHandler(ICommandRepository<Vat> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DeleteVatResult> Handle(DeleteVatRequest request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);

            if (entity == null)
            {
                throw new Exception($"Entity not found: {request.Id}");
            }

            entity.IsDeleted = true;

             _repository.Delete(entity);
            await _unitOfWork.SaveAsync();

            return new DeleteVatResult
            {
                Id = entity.Id,
                IsDeleted = true
            };
        }
    }
}

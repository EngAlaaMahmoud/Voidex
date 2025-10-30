using Application.Common.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.PurchaseOrderManager.Commands
{
    public class UpdatePurchaseOrderDiscountResult
    {
        public PurchaseOrder? Data { get; set; }
    }

    public class UpdatePurchaseOrderDiscountRequest : IRequest<UpdatePurchaseOrderDiscountResult>
    {
        public string? Id { get; init; }
        public double? Discount { get; init; }
        public string? UpdatedById { get; init; }
    }

    public class UpdatePurchaseOrderDiscountValidator : AbstractValidator<UpdatePurchaseOrderDiscountRequest>
    {
        public UpdatePurchaseOrderDiscountValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Discount).GreaterThanOrEqualTo(0)
                .WithMessage("Discount must be >= 0");
        }
    }

    public class UpdatePurchaseOrderDiscountHandler :
        IRequestHandler<UpdatePurchaseOrderDiscountRequest, UpdatePurchaseOrderDiscountResult>
    {
        private readonly ICommandRepository<PurchaseOrder> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PurchaseOrderService _purchaseOrderService;

        public UpdatePurchaseOrderDiscountHandler(
            ICommandRepository<PurchaseOrder> repository,
            IUnitOfWork unitOfWork,
            PurchaseOrderService purchaseOrderService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _purchaseOrderService = purchaseOrderService;
        }

        public async Task<UpdatePurchaseOrderDiscountResult> Handle(
            UpdatePurchaseOrderDiscountRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _repository.GetQuery()
                .FirstOrDefaultAsync(x => x.Id == (request.Id ?? string.Empty), cancellationToken);

            if (entity == null)
                throw new Exception($"Purchase order not found: {request.Id}");

            entity.UpdatedById = request.UpdatedById;
            entity.Discount = request.Discount ?? 0;

            _repository.Update(entity);
            await _unitOfWork.SaveAsync(cancellationToken);

            _purchaseOrderService.Recalculate(entity.Id);

            return new UpdatePurchaseOrderDiscountResult { Data = entity };
        }
    }
}

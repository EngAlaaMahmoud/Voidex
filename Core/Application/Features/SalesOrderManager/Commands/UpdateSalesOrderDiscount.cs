using Application.Common.CQS.Commands;
using Application.Common.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Application.Features.SalesOrderManager.Commands
{
    public class UpdateSalesOrderDiscountResult
    {
        public SalesOrder? Data { get; set; }
    }

    public class UpdateSalesOrderDiscountRequest : IRequest<UpdateSalesOrderDiscountResult>
    {
        public string? Id { get; init; }
        public double? Discount { get; init; }
        public string? UpdatedById { get; init; }
    }

    public class UpdateSalesOrderDiscountValidator : AbstractValidator<UpdateSalesOrderDiscountRequest>
    {
        public UpdateSalesOrderDiscountValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount must be greater than or equal to 0")
               // .LessThanOrEqualTo(100).WithMessage("Discount must be less than or equal to 100")
                ;
        }
    }

    public class UpdateSalesOrderDiscountHandler : IRequestHandler<UpdateSalesOrderDiscountRequest, UpdateSalesOrderDiscountResult>
    {
        private readonly ICommandRepository<SalesOrder> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly SalesOrderService _salesOrderService;

        public UpdateSalesOrderDiscountHandler(
            ICommandRepository<SalesOrder> repository,
            SalesOrderService salesOrderService,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _salesOrderService = salesOrderService;
        }

        public async Task<UpdateSalesOrderDiscountResult> Handle(UpdateSalesOrderDiscountRequest request, CancellationToken cancellationToken)
        {
            // Load the sales order
            var entity = await _repository
                .GetQuery()
                .FirstOrDefaultAsync(x => x.Id == (request.Id ?? string.Empty), cancellationToken);

            if (entity == null)
                throw new Exception($"Sales order not found: {request.Id}");

            // Update only the discount and UpdatedById
            entity.UpdatedById = request.UpdatedById;
            entity.Discount = request.Discount ?? 0;

            _repository.Update(entity);
            await _unitOfWork.SaveAsync(cancellationToken);

            // Recalculate the order totals
            _salesOrderService.Recalculate(entity.Id);

            return new UpdateSalesOrderDiscountResult
            {
                Data = entity
            };
        }
    }
}
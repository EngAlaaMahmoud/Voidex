using Application.Common.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.ProductCompanyManager.Commands;

public class DeleteProductCompanyResult
{
    public ProductCompany? Data { get; set; }
}

public class DeleteProductCompanyRequest : IRequest<DeleteProductCompanyResult>
{
    public string? Id { get; init; }
    public string? DeletedById { get; init; }
}

public class DeleteProductCompanyValidator : AbstractValidator<DeleteProductCompanyRequest>
{
    public DeleteProductCompanyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteProductCompanyHandler : IRequestHandler<DeleteProductCompanyRequest, DeleteProductCompanyResult>
{
    private readonly ICommandRepository<ProductCompany> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCompanyHandler(ICommandRepository<ProductCompany> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteProductCompanyResult> Handle(DeleteProductCompanyRequest request, CancellationToken cancellationToken)
    {
        // if not found, return null result instead of throwing
        var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);
        if (entity == null)
        {
            return new DeleteProductCompanyResult { Data = null };
        }

        // prefer soft-delete if entity contains IsDeleted flag; otherwise delete
        try
        {
            var prop = entity.GetType().GetProperty("IsDeleted");
            if (prop != null)
            {
                prop.SetValue(entity, true);
                var updatedByProp = entity.GetType().GetProperty("UpdatedById");
                var updatedAtProp = entity.GetType().GetProperty("UpdatedAtUtc");
                if (updatedByProp != null) updatedByProp.SetValue(entity, request.DeletedById);
                if (updatedAtProp != null) updatedAtProp.SetValue(entity, DateTime.UtcNow);
                _repository.Update(entity);
            }
            else
            {
                _repository.Delete(entity);
            }

            await _unitOfWork.SaveAsync(cancellationToken);
        }
        catch
        {
            // fallback: attempt delete if update fails
            _repository.Delete(entity);
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        return new DeleteProductCompanyResult { Data = entity };
    }
}

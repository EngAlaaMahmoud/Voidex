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
        var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);
        if (entity == null) throw new Exception($"Entity not found: {request.Id}");

        _repository.Delete(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new DeleteProductCompanyResult { Data = entity };
    }
}

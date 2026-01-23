using Application.Common.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.ProductCompanyManager.Commands;

public class UpdateProductCompanyResult
{
    public ProductCompany? Data { get; set; }
}

public class UpdateProductCompanyRequest : IRequest<UpdateProductCompanyResult>
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? Description { get; init; }
    public string? UpdatedById { get; init; }
}

public class UpdateProductCompanyValidator : AbstractValidator<UpdateProductCompanyRequest>
{
    public UpdateProductCompanyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class UpdateProductCompanyHandler : IRequestHandler<UpdateProductCompanyRequest, UpdateProductCompanyResult>
{
    private readonly ICommandRepository<ProductCompany> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCompanyHandler(ICommandRepository<ProductCompany> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateProductCompanyResult> Handle(UpdateProductCompanyRequest request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetAsync(request.Id ?? string.Empty, cancellationToken);
        if (entity == null) throw new Exception($"Entity not found: {request.Id}");

        entity.Name = request.Name;
        entity.Street = request.Street;
        entity.City = request.City;
        entity.Description = request.Description;
        entity.UpdatedById = request.UpdatedById;

        _repository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new UpdateProductCompanyResult { Data = entity };
    }
}

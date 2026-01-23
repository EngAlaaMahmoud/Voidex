using Application.Common.Repositories;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.ProductCompanyManager.Commands;

public class CreateProductCompanyResult
{
    public ProductCompany? Data { get; set; }
}

public class CreateProductCompanyRequest : IRequest<CreateProductCompanyResult>
{
    public string? Name { get; init; }
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? Description { get; init; }
    public string? CreatedById { get; init; }
}

public class CreateProductCompanyValidator : AbstractValidator<CreateProductCompanyRequest>
{
    public CreateProductCompanyValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class CreateProductCompanyHandler : IRequestHandler<CreateProductCompanyRequest, CreateProductCompanyResult>
{
    private readonly ICommandRepository<ProductCompany> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCompanyHandler(ICommandRepository<ProductCompany> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateProductCompanyResult> Handle(CreateProductCompanyRequest request, CancellationToken cancellationToken)
    {
        var entity = new ProductCompany
        {
            Name = request.Name,
            Street = request.Street,
            City = request.City,
            Description = request.Description,
            CreatedById = request.CreatedById,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _repository.CreateAsync(entity, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new CreateProductCompanyResult { Data = entity };
    }
}

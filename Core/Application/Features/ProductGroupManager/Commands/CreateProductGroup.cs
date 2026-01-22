using Application.Common.Repositories;
using Application.Common.CQS.Queries;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.ProductGroupManager.Commands;

public class CreateProductGroupResult
{
    public ProductGroup? Data { get; set; }
}

public class CreateProductGroupRequest : IRequest<CreateProductGroupResult>
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<string>? CompanyIds { get; init; }
    public List<string>? CompanyNames { get; init; }
    public List<CompanyCreateDto>? CompanyRecords { get; init; }
    public string? CreatedById { get; init; }
}

public record CompanyCreateDto
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? Description { get; init; }
}

public class CreateProductGroupValidator : AbstractValidator<CreateProductGroupRequest>
{
    public CreateProductGroupValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class CreateProductGroupHandler : IRequestHandler<CreateProductGroupRequest, CreateProductGroupResult>
{
    private readonly ICommandRepository<ProductGroup> _repository;
    private readonly ICommandRepository<ProductCompany> _productCompanyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryContext _queryContext;

    public CreateProductGroupHandler(
        ICommandRepository<ProductGroup> repository,
        ICommandRepository<ProductCompany> productCompanyRepository,
        IUnitOfWork unitOfWork,
        IQueryContext queryContext
        )
    {
        _repository = repository;
        _productCompanyRepository = productCompanyRepository;
        _unitOfWork = unitOfWork;
        _queryContext = queryContext;
    }

    public async Task<CreateProductGroupResult> Handle(CreateProductGroupRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new ProductGroup();
        entity.CreatedById = request.CreatedById;

        entity.Name = request.Name;
        entity.Description = request.Description;
        var companies = new List<ProductCompany>();
        if (request.CompanyIds != null && request.CompanyIds.Any())
        {
            var exist = await _productCompanyRepository.GetQuery().Where(c => request.CompanyIds.Contains(c.Id)).ToListAsync(cancellationToken);
            companies.AddRange(exist);
        }

        if (request.CompanyNames != null && request.CompanyNames.Any())
        {
            foreach (var raw in request.CompanyNames)
            {
                var name = raw?.Trim();
                if (string.IsNullOrEmpty(name)) continue;
                var existing = await _productCompanyRepository.GetQuery().FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
                if (existing != null)
                {
                    if (!companies.Any(c => c.Id == existing.Id)) companies.Add(existing);
                }
                else
                {
                    var newCompany = new ProductCompany { Name = name, CreatedAtUtc = DateTime.UtcNow };
                    await _productCompanyRepository.CreateAsync(newCompany, cancellationToken);
                    companies.Add(newCompany);
                }
            }
        }

        // process full company records (name + address)
        if (request.CompanyRecords != null && request.CompanyRecords.Any())
        {
            foreach (var rec in request.CompanyRecords)
            {
                if (rec == null) continue;
                if (!string.IsNullOrWhiteSpace(rec.Id))
                {
                    var existById = await _productCompanyRepository.GetQuery().FirstOrDefaultAsync(c => c.Id == rec.Id, cancellationToken);
                    if (existById != null && !companies.Any(c => c.Id == existById.Id))
                    {
                        companies.Add(existById);
                        continue;
                    }
                }

                if (!string.IsNullOrWhiteSpace(rec.Name))
                {
                    var existByName = await _productCompanyRepository.GetQuery().FirstOrDefaultAsync(c => c.Name == rec.Name, cancellationToken);
                    if (existByName != null)
                    {
                        if (!companies.Any(c => c.Id == existByName.Id)) companies.Add(existByName);
                        continue;
                    }

                    var newCompany = new ProductCompany
                    {
                        Name = rec.Name,
                        Street = rec.Street,
                        City = rec.City,
                        Description = rec.Description,
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    await _productCompanyRepository.CreateAsync(newCompany, cancellationToken);
                    companies.Add(newCompany);
                }
            }
        }

        if (companies.Any())
        {
            entity.ProductCompanies = companies;
        }

        await _repository.CreateAsync(entity, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new CreateProductGroupResult
        {
            Data = entity
        };
    }
}
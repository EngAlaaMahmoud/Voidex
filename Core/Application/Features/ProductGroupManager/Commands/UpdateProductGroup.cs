using Application.Common.Repositories;
using Application.Common.CQS.Queries;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.ProductGroupManager.Commands;

public class UpdateProductGroupResult
{
    public ProductGroup? Data { get; set; }
}

public class UpdateProductGroupRequest : IRequest<UpdateProductGroupResult>
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public List<string>? CompanyIds { get; init; }
    public List<string>? CompanyNames { get; init; }
    public List<CompanyCreateDto>? CompanyRecords { get; init; }
    public string? UpdatedById { get; init; }
}

public class UpdateProductGroupValidator : AbstractValidator<UpdateProductGroupRequest>
{
    public UpdateProductGroupValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class UpdateProductGroupHandler : IRequestHandler<UpdateProductGroupRequest, UpdateProductGroupResult>
{
    private readonly ICommandRepository<ProductGroup> _repository;
    private readonly ICommandRepository<ProductCompany> _productCompanyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryContext _queryContext;

    public UpdateProductGroupHandler(
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

    public async Task<UpdateProductGroupResult> Handle(UpdateProductGroupRequest request, CancellationToken cancellationToken)
    {

        var entity = await _repository.GetQuery()
            .Include(x => x.ProductCompanies)
            .FirstOrDefaultAsync(x => x.Id == (request.Id ?? string.Empty), cancellationToken);

        if (entity == null)
        {
            throw new Exception($"Entity not found: {request.Id}");
        }

        entity.UpdatedById = request.UpdatedById;

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
            entity.ProductCompanies.Clear();
            foreach (var c in companies)
            {
                entity.ProductCompanies.Add(c);
            }
        }

        _repository.Update(entity);
        await _unitOfWork.SaveAsync(cancellationToken);

        return new UpdateProductGroupResult
        {
            Data = entity
        };
    }
}


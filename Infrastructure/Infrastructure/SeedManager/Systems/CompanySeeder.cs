using Application.Common.Repositories;
using Domain.Entities;

namespace Infrastructure.SeedManager.Systems;

public class CompanySeeder
{
    private readonly ICommandRepository<Company> _repository;
    private readonly IUnitOfWork _unitOfWork;
    public CompanySeeder(
        ICommandRepository<Company> repository,
        IUnitOfWork unitOfWork
        )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }
    public async Task GenerateDataAsync()
    {
        // Seed a minimal set of companies (names only) if they do not exist yet
        var companyNames = new[] { "Acme Corp", "Samsung", "Apple", "Nokia", "Huawei" };

        foreach (var name in companyNames)
        {
            var exists = _repository.GetQuery().Any(x => x.Name == name);
            if (!exists)
            {
                var entity = new Company
                {
                    CreatedAtUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    Name = name
                };

                await _repository.CreateAsync(entity);
            }
        }

        await _unitOfWork.SaveAsync();
    }

}

using Application.Common.Repositories;
using Domain.Entities;

namespace Infrastructure.SeedManager.Demos
{
    public class VatSeeder
    {
        private readonly ICommandRepository<Vat> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public VatSeeder(
            ICommandRepository<Vat> repository,
            IUnitOfWork unitOfWork
        )
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task GenerateDataAsync()
        {
            var vats = new List<Vat>
        {
            new Vat { Name = "NOVAT", Percentage = 0.0 },
            new Vat { Name = "V10", Percentage = 10.0 },
            new Vat { Name = "V15", Percentage = 15.0 },
            new Vat { Name = "V20", Percentage = 20.0 }
        };

            foreach (var vat in vats)
            {
                await _repository.CreateAsync(vat);
            }

            await _unitOfWork.SaveAsync();
        }
    }
}

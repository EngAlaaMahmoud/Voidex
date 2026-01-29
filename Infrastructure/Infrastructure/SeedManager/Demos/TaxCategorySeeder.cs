using Application.Common.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SeedManager.Demos;

public class TaxCategorySeeder
{
    private readonly ICommandRepository<TaxCategory> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TaxCategorySeeder(
        ICommandRepository<TaxCategory> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedAsync()
    {
        // Check if already seeded (avoid duplicates)
        var existing = await _repository.GetQuery()
            .IgnoreQueryFilters()
            .Select(c => c.Code)
            .ToListAsync();

        var categories = new List<TaxCategory>
        {
            new() { Code = "VA", NameAr = "ضريبة القيمة المضافة",     NameEn = "Value Added Tax",         SortOrder = 1 },
            new() { Code = "TB", NameAr = "ضريبة الجدول",             NameEn = "Table Tax",               SortOrder = 2 },
            new() { Code = "WH", NameAr = "خصم تحت حساب الضريبة",     NameEn = "Withholding Tax",         SortOrder = 3 },
            new() { Code = "ST", NameAr = "ضريبة الدمغة",             NameEn = "Stamp Duty",              SortOrder = 4 },
            new() { Code = "EN", NameAr = "ضريبة الملاهي",            NameEn = "Entertainment Tax",       SortOrder = 5 },
            new() { Code = "SE", NameAr = "رسوم الخدمة",              NameEn = "Service Fee",             SortOrder = 6 },
            new() { Code = "HI", NameAr = "رسوم التأمين الصحي",       NameEn = "Health Insurance Fee",    SortOrder = 7 },
            new() { Code = "RD", NameAr = "رسوم تنمية الموارد",       NameEn = "Resource Development Fee",SortOrder = 8 },
            new() { Code = "LO", NameAr = "رسوم المحليات",            NameEn = "Local Fees",              SortOrder = 9 },
            new() { Code = "OT", NameAr = "رسوم أخرى",                NameEn = "Other Fees",              SortOrder = 10 },
        };

        bool anyAdded = false;

        foreach (var category in categories)
        {
            if (!existing.Contains(category.Code))
            {
                await _repository.CreateAsync(category);
                anyAdded = true;
            }
        }

        if (anyAdded)
        {
            await _unitOfWork.SaveAsync();
            // Optional: log success
            Console.WriteLine("Tax categories seeded successfully.");
        }
        else
        {
            Console.WriteLine("Tax categories already exist - skipping seed.");
        }
    }
}
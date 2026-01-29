using Application.Common.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.SeedManager.Demos;

public class TaxSeeder
{
    private readonly ICommandRepository<TaxCategory> _categoryRepo;
    private readonly ICommandRepository<Tax> _taxRepo;
    private readonly IUnitOfWork _unitOfWork;

    public TaxSeeder(
        ICommandRepository<TaxCategory> categoryRepo,
        ICommandRepository<Tax> taxRepo,
        IUnitOfWork unitOfWork)
    {
        _categoryRepo = categoryRepo;
        _taxRepo = taxRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task GenerateDataAsync()
    {
        // ────────────────────────────────────────────────
        // 1. Seed Tax Categories (idempotent)
        // ────────────────────────────────────────────────
        var existingCategoryCodes = await _categoryRepo.GetQuery()
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
            new() { Code = "SE", NameAr = "رسوم الخدمة",              NameEn = "Service Fees",            SortOrder = 6 },
            new() { Code = "HI", NameAr = "رسوم التأمين الصحي",       NameEn = "Health Insurance Fees",   SortOrder = 7 },
            new() { Code = "RD", NameAr = "رسوم تنمية الموارد",       NameEn = "Resource Development Fees", SortOrder = 8 },
            new() { Code = "LO", NameAr = "رسوم المحليات",            NameEn = "Local Fees",              SortOrder = 9 },
            new() { Code = "OT", NameAr = "رسوم أخرى",                NameEn = "Other Fees",              SortOrder = 10 },
        };

        bool categoryAdded = false;
        foreach (var cat in categories)
        {
            if (!existingCategoryCodes.Contains(cat.Code))
            {
                await _categoryRepo.CreateAsync(cat);
                categoryAdded = true;
            }
        }

        if (categoryAdded)
        {
            await _unitOfWork.SaveAsync();
        }

        // ────────────────────────────────────────────────
        // 2. Load category map (Code → Id)
        // ────────────────────────────────────────────────
        var categoryMap = new Dictionary<string, string>();
        var allCategories = await _categoryRepo.GetQuery()
            .Select(c => new { c.Id, c.Code })
            .ToListAsync();

        foreach (var c in allCategories)
        {
            categoryMap[c.Code] = c.Id;
        }

        // Safety check
        var requiredCodes = new[] { "VA", "TB", "WH", "ST", "EN", "SE", "HI", "RD", "LO", "OT" };
        foreach (var code in requiredCodes)
        {
            if (!categoryMap.ContainsKey(code))
            {
                throw new InvalidOperationException($"Category {code} missing after seeding.");
            }
        }

        // ────────────────────────────────────────────────
        // 3. Seed all Taxes (full list from your data)
        // ────────────────────────────────────────────────
        var taxes = new List<Tax>
        {
            // T1 → VA
            new() { MainCode = "T1", SubCode = "V001", TaxType = "VAT", Description = "تصدير للخارج", Percentage = 0, TaxCategoryId = categoryMap["VA"] },
            new() { MainCode = "T1", SubCode = "V002", TaxType = "VAT", Description = "تصدير لمناطق حرة وأخرى", Percentage = 0, TaxCategoryId = categoryMap["VA"] },
            new() { MainCode = "T1", SubCode = "V003", TaxType = "VAT", Description = "سلعة أو خدمة معفاة", Percentage = 0, TaxCategoryId = categoryMap["VA"] },
            new() { MainCode = "T1", SubCode = "V004", TaxType = "VAT", Description = "سلعة أو خدمة غير خاضعة", Percentage = 0, TaxCategoryId = categoryMap["VA"] },
            new() { MainCode = "T1", SubCode = "V005", TaxType = "VAT", Description = "لبيان", Percentage = 0, TaxCategoryId = categoryMap["VA"] },
            new() { MainCode = "T1", SubCode = "V006", TaxType = "VAT", Description = "إعفاءات دفاع وأمن قومي", Percentage = 0, TaxCategoryId = categoryMap["VA"] },
            new() { MainCode = "T1", SubCode = "V007", TaxType = "VAT", Description = "إعفاءات اتفاقيات", Percentage = 0, TaxCategoryId = categoryMap["VA"] },
            new() { MainCode = "T1", SubCode = "V008", TaxType = "VAT", Description = "إعفاءات أخرى", Percentage = 0, TaxCategoryId = categoryMap["VA"] },
            new() { MainCode = "T1", SubCode = "V009", TaxType = "VAT", Description = "سلع عامة", Percentage = 14, TaxCategoryId = categoryMap["VA"] },
            new() { MainCode = "T1", SubCode = "V010", TaxType = "VAT", Description = "نسب أخرى", Percentage = null, TaxCategoryId = categoryMap["VA"] },

            // T2 & T3 → TB
            new() { MainCode = "T2", SubCode = "Tbl01", TaxType = "ضريبه جدول", Description = "ضريبة الجدول – نسبية", Percentage = null, TaxCategoryId = categoryMap["TB"] },
            new() { MainCode = "T3", SubCode = "Tbl02", TaxType = "ضريبه جدول", Description = "ضريبة الجدول – نوعية", Percentage = null, TaxCategoryId = categoryMap["TB"] },

            // T4 → WH (withholding)
            new() { MainCode = "T4", SubCode = "W001", TaxType = "الخصم تحت حساب ضريبه", Description = "المقاولات", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W002", TaxType = "الخصم تحت حساب ضريبه", Description = "التوريدات", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W003", TaxType = "الخصم تحت حساب ضريبه", Description = "المشتريات", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W004", TaxType = "الخصم تحت حساب ضريبه", Description = "الخدمات", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W005", TaxType = "الخصم تحت حساب ضريبه", Description = "الجمعيات التعاونية للنقل", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W006", TaxType = "الخصم تحت حساب ضريبه", Description = "الوكالة والسمسرة", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W007", TaxType = "الخصم تحت حساب ضريبه", Description = "شركات الدخان والاسمنت", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W008", TaxType = "الخصم تحت حساب ضريبه", Description = "شركات البترول والاتصالات", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W009", TaxType = "الخصم تحت حساب ضريبه", Description = "دعم الصادرات", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W010", TaxType = "الخصم تحت حساب ضريبه", Description = "أتعاب مهنية", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W011", TaxType = "الخصم تحت حساب ضريبه", Description = "عمولة وسمسرة", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W012", TaxType = "الخصم تحت حساب ضريبه", Description = "تحصيل المستشفيات من الأطباء", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W013", TaxType = "الخصم تحت حساب ضريبه", Description = "إتاوات", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W014", TaxType = "الخصم تحت حساب ضريبه", Description = "تخليص جمركي", Percentage = 1, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W015", TaxType = "الخصم تحت حساب ضريبه", Description = "إعفاء", Percentage = 0, TaxCategoryId = categoryMap["WH"] },
            new() { MainCode = "T4", SubCode = "W016", TaxType = "الخصم تحت حساب ضريبه", Description = "دفعات مقدمة", Percentage = 0, TaxCategoryId = categoryMap["WH"] },

            // T5, T6, T13, T14 → ST (Stamp)
            new() { MainCode = "T5", SubCode = "ST01", TaxType = "ضريبه الدمغه", Description = "ضريبة الدمغة – نسبية", Percentage = null, TaxCategoryId = categoryMap["ST"] },
            new() { MainCode = "T6", SubCode = "ST02", TaxType = "ضريبه الدمغه", Description = "ضريبة الدمغة – قطعية", Percentage = null, TaxCategoryId = categoryMap["ST"] },
            new() { MainCode = "T13", SubCode = "ST03", TaxType = "ضريبه دمغه", Description = "ضريبة الدمغة نسبية (غير ضريبي)", Percentage = null, TaxCategoryId = categoryMap["ST"] },
            new() { MainCode = "T14", SubCode = "ST04", TaxType = "ضريبه دمغه", Description = "ضريبة الدمغة قطعية (غير ضريبي)", Percentage = null, TaxCategoryId = categoryMap["ST"] },

            // T7, T15 → EN (Entertainment)
            new() { MainCode = "T7", SubCode = "Ent01", TaxType = "ضريبه الملاهى", Description = "ضريبة الملاهي نسبية", Percentage = null, TaxCategoryId = categoryMap["EN"] },
            new() { MainCode = "T7", SubCode = "Ent02", TaxType = "ضريبه الملاهى", Description = "ضريبة الملاهي قطعية", Percentage = null, TaxCategoryId = categoryMap["EN"] },
            new() { MainCode = "T15", SubCode = "Ent03", TaxType = "ضريبه ملاهى", Description = "ضريبة الملاهي نسبية", Percentage = null, TaxCategoryId = categoryMap["EN"] },
            new() { MainCode = "T15", SubCode = "Ent04", TaxType = "ضريبه ملاهى", Description = "ضريبة الملاهي قطعية", Percentage = null, TaxCategoryId = categoryMap["EN"] },

            // T8, T16 → RD (Resource Development)
            new() { MainCode = "T8", SubCode = "RD01", TaxType = "رسم تنميه الموارد", Description = "رسم تنمية الموارد – نسبية", Percentage = null, TaxCategoryId = categoryMap["RD"] },
            new() { MainCode = "T8", SubCode = "RD02", TaxType = "رسم تنميه الموارد", Description = "رسم تنمية الموارد – قطعية", Percentage = null, TaxCategoryId = categoryMap["RD"] },
            new() { MainCode = "T16", SubCode = "RD03", TaxType = "رسوم تنميه الموارد", Description = "رسم تنمية الموارد – نسبة", Percentage = null, TaxCategoryId = categoryMap["RD"] },
            new() { MainCode = "T16", SubCode = "RD04", TaxType = "رسوم تنميه الموارد", Description = "رسم تنمية الموارد – قطعية", Percentage = null, TaxCategoryId = categoryMap["RD"] },

            // T9, T17 → SE (Service Fees)
            new() { MainCode = "T9", SubCode = "RD01", TaxType = "رسم خدمه", Description = "رسم خدمة – نسبية", Percentage = null, TaxCategoryId = categoryMap["SE"] },
            new() { MainCode = "T9", SubCode = "RD02", TaxType = "رسم خدمه", Description = "رسم خدمة – قطعية", Percentage = null, TaxCategoryId = categoryMap["SE"] },
            new() { MainCode = "T17", SubCode = "SC03", TaxType = "رسوم خدمه", Description = "رسم خدمة – نسبة", Percentage = null, TaxCategoryId = categoryMap["SE"] },
            new() { MainCode = "T17", SubCode = "SC04", TaxType = "رسوم خدمه", Description = "رسم خدمة – قطعية", Percentage = null, TaxCategoryId = categoryMap["SE"] },

            // T10, T18 → LO (Local Fees)
            new() { MainCode = "T10", SubCode = "Mn01", TaxType = "رسم محليات", Description = "رسم المحليات – نسبة", Percentage = null, TaxCategoryId = categoryMap["LO"] },
            new() { MainCode = "T10", SubCode = "Mn02", TaxType = "رسم محليات", Description = "رسم المحليات – قطعية", Percentage = null, TaxCategoryId = categoryMap["LO"] },
            new() { MainCode = "T18", SubCode = "Mn03", TaxType = "رسم المحليات", Description = "رسم المحليات – نسبة", Percentage = null, TaxCategoryId = categoryMap["LO"] },
            new() { MainCode = "T18", SubCode = "Mn04", TaxType = "رسم المحليات", Description = "رسم المحليات – قطعية", Percentage = null, TaxCategoryId = categoryMap["LO"] },

            // T11, T19 → HI (Health Insurance)
            new() { MainCode = "T11", SubCode = "MI01", TaxType = "رسم التامين الصحى", Description = "رسم التأمين الصحي – نسبة", Percentage = null, TaxCategoryId = categoryMap["HI"] },
            new() { MainCode = "T11", SubCode = "MI02", TaxType = "رسم التامين الصحى", Description = "رسم التأمين الصحي – قطعية", Percentage = null, TaxCategoryId = categoryMap["HI"] },
            new() { MainCode = "T19", SubCode = "MI03", TaxType = "رسم التامين الصحى", Description = "رسم التأمين الصحي – نسبة", Percentage = null, TaxCategoryId = categoryMap["HI"] },
            new() { MainCode = "T19", SubCode = "MI04", TaxType = "رسوم التامين الصحى", Description = "رسم التأمين الصحي – قطعية", Percentage = null, TaxCategoryId = categoryMap["HI"] },

            // T12, T20 → OT (Other Fees)
            new() { MainCode = "T12", SubCode = "OF01", TaxType = "رسوم اخرى", Description = "رسوم أخرى – نسبة", Percentage = null, TaxCategoryId = categoryMap["OT"] },
            new() { MainCode = "T12", SubCode = "OF02", TaxType = "رسوم اخرى", Description = "رسوم أخرى – قطعية", Percentage = null, TaxCategoryId = categoryMap["OT"] },
            new() { MainCode = "T20", SubCode = "OF03", TaxType = "رسوم أخرى", Description = "رسوم أخرى – نسبة", Percentage = null, TaxCategoryId = categoryMap["OT"] },
            new() { MainCode = "T20", SubCode = "OF04", TaxType = "رسوم أخرى", Description = "رسوم أخرى – قطعية", Percentage = null, TaxCategoryId = categoryMap["OT"] },
        };

        // Optional: check existing taxes to avoid duplicates (by MainCode + SubCode)
        var existingTaxKeys = await _taxRepo.GetQuery()
            .Select(t => new { t.MainCode, t.SubCode })
            .ToListAsync();

        var existingSet = new HashSet<(string, string)>(existingTaxKeys.Select(k => (k.MainCode ?? "", k.SubCode ?? "")));

        bool taxAdded = false;
        foreach (var tax in taxes)
        {
            var key = (tax.MainCode ?? "", tax.SubCode ?? "");
            if (!existingSet.Contains(key))
            {
                await _taxRepo.CreateAsync(tax);
                taxAdded = true;
            }
        }

        if (taxAdded || categoryAdded)
        {
            await _unitOfWork.SaveAsync();
        }
    }
}
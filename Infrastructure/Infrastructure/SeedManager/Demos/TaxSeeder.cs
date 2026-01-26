using Application.Common.Repositories;
using Domain.Entities;

namespace Infrastructure.SeedManager.Demos;

public class TaxSeeder
{
    private readonly ICommandRepository<Tax> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TaxSeeder(
        ICommandRepository<Tax> repository,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task GenerateDataAsync()
    {
        var taxes = new List<Tax>
        {
            // Add tax register rows provided by user
            new Tax { MainCode = "T1", SubCode = "V001", TaxType = "VAT", Description = "تصدير للخارج" },
            new Tax { MainCode = "T1", SubCode = "V002", TaxType = "VAT", Description = "تصدير لمناطق حرة وأخرى" },
            new Tax { MainCode = "T1", SubCode = "V003", TaxType = "VAT", Description = "سلعة أو خدمة معفاة" },
            new Tax { MainCode = "T1", SubCode = "V004", TaxType = "VAT", Description = "سلعة أو خدمة غير خاضعة" },
            new Tax { MainCode = "T1", SubCode = "V005", TaxType = "VAT", Description = "لبيان" },
            new Tax { MainCode = "T1", SubCode = "V006", TaxType = "VAT", Description = "إعفاءات دفاع وأمن قومي" },
            new Tax { MainCode = "T1", SubCode = "V007", TaxType = "VAT", Description = "إعفاءات اتفاقيات" },
            new Tax { MainCode = "T1", SubCode = "V008", TaxType = "VAT", Description = "إعفاءات أخرى" },
            new Tax { MainCode = "T1", SubCode = "V009", TaxType = "VAT", Description = "سلع عامة" },
            new Tax { MainCode = "T1", SubCode = "V010", TaxType = "VAT", Description = "نسب أخرى" },
            new Tax { MainCode = "T2", SubCode = "Tbl01", TaxType = "ضريبه جدول", Description = "ضريبة الجدول – نسبية" },
            new Tax { MainCode = "T3", SubCode = "Tbl02", TaxType = "ضريبه جدول", Description = "ضريبة الجدول – نوعية" },
            new Tax { MainCode = "T4", SubCode = "W001", TaxType = "الخصم تحت حساب ضريبه", Description = "المقاولات" },
            new Tax { MainCode = "T4", SubCode = "W002", TaxType = "الخصم تحت حساب ضريبه", Description = "التوريدات" },
            new Tax { MainCode = "T4", SubCode = "W003", TaxType = "الخصم تحت حساب ضريبه", Description = "المشتريات" },
            new Tax { MainCode = "T4", SubCode = "W004", TaxType = "الخصم تحت حساب ضريبه", Description = "الخدمات" },
            new Tax { MainCode = "T4", SubCode = "W005", TaxType = "الخصم تحت حساب ضريبه", Description = "الجمعيات التعاونية للنقل" },
            new Tax { MainCode = "T4", SubCode = "W006", TaxType = "الخصم تحت حساب ضريبه", Description = "الوكالة والسمسرة" },
            new Tax { MainCode = "T4", SubCode = "W007", TaxType = "الخصم تحت حساب ضريبه", Description = "شركات الدخان والاسمنت" },
            new Tax { MainCode = "T4", SubCode = "W008", TaxType = "الخصم تحت حساب ضريبه", Description = "شركات البترول والاتصالات" },
            new Tax { MainCode = "T4", SubCode = "W009", TaxType = "الخصم تحت حساب ضريبه", Description = "دعم الصادرات" },
            new Tax { MainCode = "T4", SubCode = "W010", TaxType = "الخصم تحت حساب ضريبه", Description = "أتعاب مهنية" },
            new Tax { MainCode = "T4", SubCode = "W011", TaxType = "الخصم تحت حساب ضريبه", Description = "عمولة وسمسرة" },
            new Tax { MainCode = "T4", SubCode = "W012", TaxType = "الخصم تحت حساب ضريبه", Description = "تحصيل المستشفيات من الأطباء" },
            new Tax { MainCode = "T4", SubCode = "W013", TaxType = "الخصم تحت حساب ضريبه", Description = "إتاوات" },
            new Tax { MainCode = "T4", SubCode = "W014", TaxType = "الخصم تحت حساب ضريبه", Description = "تخليص جمركي" },
            new Tax { MainCode = "T4", SubCode = "W015", TaxType = "الخصم تحت حساب ضريبه", Description = "إعفاء" },
            new Tax { MainCode = "T4", SubCode = "W016", TaxType = "الخصم تحت حساب ضريبه", Description = "دفعات مقدمة" },
            new Tax { MainCode = "T5", SubCode = "ST01", TaxType = "ضريبه الدمغه", Description = "ضريبة الدمغة – نسبية" },
            new Tax { MainCode = "T6", SubCode = "ST02", TaxType = "ضريبه الدمغه", Description = "ضريبة الدمغة – قطعية" },
            new Tax { MainCode = "T7", SubCode = "Ent01", TaxType = "ضريبه الملاهى", Description = "ضريبة الملاهي نسبية" },
            new Tax { MainCode = "T7", SubCode = "Ent02", TaxType = "ضريبه الملاهى", Description = "ضريبة الملاهي قطعية" },
            new Tax { MainCode = "T8", SubCode = "RD01", TaxType = "رسم تنميه الموارد", Description = "رسم تنمية الموارد – نسبية" },
            new Tax { MainCode = "T8", SubCode = "RD02", TaxType = "رسم تنميه الموارد", Description = "رسم تنمية الموارد – قطعية" },
            new Tax { MainCode = "T9", SubCode = "RD01", TaxType = "رسم خدمه", Description = "رسم خدمة – نسبية" },
            new Tax { MainCode = "T9", SubCode = "RD02", TaxType = "رسم خدمه", Description = "رسم خدمة – قطعية" },
            new Tax { MainCode = "T10", SubCode = "Mn01", TaxType = "رسم محليات", Description = "رسم المحليات – نسبة" },
            new Tax { MainCode = "T10", SubCode = "Mn02", TaxType = "رسم محليات", Description = "رسم المحليات – قطعية" },
            new Tax { MainCode = "T11", SubCode = "MI01", TaxType = "رسم التامين الصحى", Description = "رسم التأمين الصحي – نسبة" },
            new Tax { MainCode = "T11", SubCode = "MI02", TaxType = "رسم التامين الصحى", Description = "رسم التأمين الصحي – قطعية" },
            new Tax { MainCode = "T12", SubCode = "OF01", TaxType = "رسوم اخرى", Description = "رسوم أخرى – نسبة" },
            new Tax { MainCode = "T12", SubCode = "OF02", TaxType = "رسوم اخرى", Description = "رسوم أخرى – قطعية" },
            new Tax { MainCode = "T13", SubCode = "ST03", TaxType = "ضريبه دمغه", Description = "ضريبة الدمغة نسبية (غير ضريبي)" },
            new Tax { MainCode = "T14", SubCode = "ST04", TaxType = "ضريبه دمغه", Description = "ضريبة الدمغة قطعية (غير ضريبي)" },
            new Tax { MainCode = "T15", SubCode = "Ent03", TaxType = "ضريبه ملاهى", Description = "ضريبة الملاهي نسبية" },
            new Tax { MainCode = "T15", SubCode = "Ent04", TaxType = "ضريبه ملاهى", Description = "ضريبة الملاهي قطعية" },
            new Tax { MainCode = "T16", SubCode = "RD03", TaxType = "رسوم تنميه الموارد", Description = "رسم تنمية الموارد – نسبة" },
            new Tax { MainCode = "T16", SubCode = "RD04", TaxType = "رسوم تنميه الموارد", Description = "رسم تنمية الموارد – قطعية" },
            new Tax { MainCode = "T17", SubCode = "SC03", TaxType = "رسوم خدمه", Description = "رسم خدمة – نسبة" },
            new Tax { MainCode = "T17", SubCode = "SC04", TaxType = "رسوم خدمه", Description = "رسم خدمة – قطعية" },
            new Tax { MainCode = "T18", SubCode = "Mn03", TaxType = "رسم المحليات", Description = "رسم المحليات – نسبة" },
            new Tax { MainCode = "T18", SubCode = "Mn04", TaxType = "رسم المحليات", Description = "رسم المحليات – قطعية" },
            new Tax { MainCode = "T19", SubCode = "MI03", TaxType = "رسال التامين الصحى", Description = "رسم التأمين الصحي – نسبة" },
            new Tax { MainCode = "T19", SubCode = "MI04", TaxType = "رسوم التامين الصحى", Description = "رسم التأمين الصحي – قطعية" },
            new Tax { MainCode = "T20", SubCode = "OF03", TaxType = "رسوم أخرى", Description = "رسوم أخرى – نسبة" },
            new Tax { MainCode = "T20", SubCode = "OF04", TaxType = "رسوم أخرى", Description = "رسوم أخرى – قطعية" },
        };

        foreach (var tax in taxes)
        {
            await _repository.CreateAsync(tax);
        }

        await _unitOfWork.SaveAsync();
    }
}


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
            new Tax { MainCode = "T1", SubCode = "V001", TypeName = "VAT", Description = "تصدير للخارج" },
            new Tax { MainCode = "T1", SubCode = "V002", TypeName = "VAT", Description = "تصدير لمناطق حرة وأخرى" },
            new Tax { MainCode = "T1", SubCode = "V003", TypeName = "VAT", Description = "سلعة أو خدمة معفاة" },
            new Tax { MainCode = "T1", SubCode = "V004", TypeName = "VAT", Description = "سلعة أو خدمة غير خاضعة" },
            new Tax { MainCode = "T1", SubCode = "V005", TypeName = "VAT", Description = "لبيان" },
            new Tax { MainCode = "T1", SubCode = "V006", TypeName = "VAT", Description = "إعفاءات دفاع وأمن قومي" },
            new Tax { MainCode = "T1", SubCode = "V007", TypeName = "VAT", Description = "إعفاءات اتفاقيات" },
            new Tax { MainCode = "T1", SubCode = "V008", TypeName = "VAT", Description = "إعفاءات أخرى" },
            new Tax { MainCode = "T1", SubCode = "V009", TypeName = "VAT", Description = "سلع عامة" },
            new Tax { MainCode = "T1", SubCode = "V010", TypeName = "VAT", Description = "نسب أخرى" },
            new Tax { MainCode = "T2", SubCode = "Tbl01", TypeName = "ضريبه جدول", Description = "ضريبة الجدول – نسبية" },
            new Tax { MainCode = "T3", SubCode = "Tbl02", TypeName = "ضريبه جدول", Description = "ضريبة الجدول – نوعية" },
            new Tax { MainCode = "T4", SubCode = "W001", TypeName = "الخصم تحت حساب ضريبه", Description = "المقاولات" },
            new Tax { MainCode = "T4", SubCode = "W002", TypeName = "الخصم تحت حساب ضريبه", Description = "التوريدات" },
            new Tax { MainCode = "T4", SubCode = "W003", TypeName = "الخصم تحت حساب ضريبه", Description = "المشتريات" },
            new Tax { MainCode = "T4", SubCode = "W004", TypeName = "الخصم تحت حساب ضريبه", Description = "الخدمات" },
            new Tax { MainCode = "T4", SubCode = "W005", TypeName = "الخصم تحت حساب ضريبه", Description = "الجمعيات التعاونية للنقل" },
            new Tax { MainCode = "T4", SubCode = "W006", TypeName = "الخصم تحت حساب ضريبه", Description = "الوكالة والسمسرة" },
            new Tax { MainCode = "T4", SubCode = "W007", TypeName = "الخصم تحت حساب ضريبه", Description = "شركات الدخان والاسمنت" },
            new Tax { MainCode = "T4", SubCode = "W008", TypeName = "الخصم تحت حساب ضريبه", Description = "شركات البترول والاتصالات" },
            new Tax { MainCode = "T4", SubCode = "W009", TypeName = "الخصم تحت حساب ضريبه", Description = "دعم الصادرات" },
            new Tax { MainCode = "T4", SubCode = "W010", TypeName = "الخصم تحت حساب ضريبه", Description = "أتعاب مهنية" },
            new Tax { MainCode = "T4", SubCode = "W011", TypeName = "الخصم تحت حساب ضريبه", Description = "عمولة وسمسرة" },
            new Tax { MainCode = "T4", SubCode = "W012", TypeName = "الخصم تحت حساب ضريبه", Description = "تحصيل المستشفيات من الأطباء" },
            new Tax { MainCode = "T4", SubCode = "W013", TypeName = "الخصم تحت حساب ضريبه", Description = "إتاوات" },
            new Tax { MainCode = "T4", SubCode = "W014", TypeName = "الخصم تحت حساب ضريبه", Description = "تخليص جمركي" },
            new Tax { MainCode = "T4", SubCode = "W015", TypeName = "الخصم تحت حساب ضريبه", Description = "إعفاء" },
            new Tax { MainCode = "T4", SubCode = "W016", TypeName = "الخصم تحت حساب ضريبه", Description = "دفعات مقدمة" },
            new Tax { MainCode = "T5", SubCode = "ST01", TypeName = "ضريبه الدمغه", Description = "ضريبة الدمغة – نسبية" },
            new Tax { MainCode = "T6", SubCode = "ST02", TypeName = "ضريبه الدمغه", Description = "ضريبة الدمغة – قطعية" },
            new Tax { MainCode = "T7", SubCode = "Ent01", TypeName = "ضريبه الملاهى", Description = "ضريبة الملاهي نسبية" },
            new Tax { MainCode = "T7", SubCode = "Ent02", TypeName = "ضريبه الملاهى", Description = "ضريبة الملاهي قطعية" },
            new Tax { MainCode = "T8", SubCode = "RD01", TypeName = "رسم تنميه الموارد", Description = "رسم تنمية الموارد – نسبية" },
            new Tax { MainCode = "T8", SubCode = "RD02", TypeName = "رسم تنميه الموارد", Description = "رسم تنمية الموارد – قطعية" },
            new Tax { MainCode = "T9", SubCode = "RD01", TypeName = "رسم خدمه", Description = "رسم خدمة – نسبية" },
            new Tax { MainCode = "T9", SubCode = "RD02", TypeName = "رسم خدمه", Description = "رسم خدمة – قطعية" },
            new Tax { MainCode = "T10", SubCode = "Mn01", TypeName = "رسم محليات", Description = "رسم المحليات – نسبة" },
            new Tax { MainCode = "T10", SubCode = "Mn02", TypeName = "رسم محليات", Description = "رسم المحليات – قطعية" },
            new Tax { MainCode = "T11", SubCode = "MI01", TypeName = "رسم التامين الصحى", Description = "رسم التأمين الصحي – نسبة" },
            new Tax { MainCode = "T11", SubCode = "MI02", TypeName = "رسم التامين الصحى", Description = "رسم التأمين الصحي – قطعية" },
            new Tax { MainCode = "T12", SubCode = "OF01", TypeName = "رسوم اخرى", Description = "رسوم أخرى – نسبة" },
            new Tax { MainCode = "T12", SubCode = "OF02", TypeName = "رسوم اخرى", Description = "رسوم أخرى – قطعية" },
            new Tax { MainCode = "T13", SubCode = "ST03", TypeName = "ضريبه دمغه", Description = "ضريبة الدمغة نسبية (غير ضريبي)" },
            new Tax { MainCode = "T14", SubCode = "ST04", TypeName = "ضريبه دمغه", Description = "ضريبة الدمغة قطعية (غير ضريبي)" },
            new Tax { MainCode = "T15", SubCode = "Ent03", TypeName = "ضريبه ملاهى", Description = "ضريبة الملاهي نسبية" },
            new Tax { MainCode = "T15", SubCode = "Ent04", TypeName = "ضريبه ملاهى", Description = "ضريبة الملاهي قطعية" },
            new Tax { MainCode = "T16", SubCode = "RD03", TypeName = "رسوم تنميه الموارد", Description = "رسم تنمية الموارد – نسبة" },
            new Tax { MainCode = "T16", SubCode = "RD04", TypeName = "رسوم تنميه الموارد", Description = "رسم تنمية الموارد – قطعية" },
            new Tax { MainCode = "T17", SubCode = "SC03", TypeName = "رسوم خدمه", Description = "رسم خدمة – نسبة" },
            new Tax { MainCode = "T17", SubCode = "SC04", TypeName = "رسوم خدمه", Description = "رسم خدمة – قطعية" },
            new Tax { MainCode = "T18", SubCode = "Mn03", TypeName = "رسم المحليات", Description = "رسم المحليات – نسبة" },
            new Tax { MainCode = "T18", SubCode = "Mn04", TypeName = "رسم المحليات", Description = "رسم المحليات – قطعية" },
            new Tax { MainCode = "T19", SubCode = "MI03", TypeName = "رسال التامين الصحى", Description = "رسم التأمين الصحي – نسبة" },
            new Tax { MainCode = "T19", SubCode = "MI04", TypeName = "رسوم التامين الصحى", Description = "رسم التأمين الصحي – قطعية" },
            new Tax { MainCode = "T20", SubCode = "OF03", TypeName = "رسوم أخرى", Description = "رسوم أخرى – نسبة" },
            new Tax { MainCode = "T20", SubCode = "OF04", TypeName = "رسوم أخرى", Description = "رسوم أخرى – قطعية" },
        };

        foreach (var tax in taxes)
        {
            await _repository.CreateAsync(tax);
        }

        await _unitOfWork.SaveAsync();
    }
}


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
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V001", TypeName = "VAT", Description = "تصدير للخارج" },
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V002", TypeName = "VAT", Description = "تصدير لمناطق حرة وأخرى" },
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V003", TypeName = "VAT", Description = "سلعة أو خدمة معفاة" },
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V004", TypeName = "VAT", Description = "سلعة أو خدمة غير خاضعة" },
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V005", TypeName = "VAT", Description = "لبيان" },
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V006", TypeName = "VAT", Description = "إعفاءات دفاع وأمن قومي" },
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V007", TypeName = "VAT", Description = "إعفاءات اتفاقيات" },
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V008", TypeName = "VAT", Description = "إعفاءات أخرى" },
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V009", TypeName = "VAT", Description = "سلع عامة" },
            new Tax { MainCode = "T_1", TypeCode = "VAT", SubCode = "V010", TypeName = "VAT", Description = "نسب أخرى" },
            new Tax { MainCode = "T_2", TypeCode = "Tbl01", SubCode = "Tbl01", TypeName = "ضريبه جدول", Description = "ضريبة الجدول – نسبية" },
            new Tax { MainCode = "T_3", TypeCode = "Tbl02", SubCode = "Tbl02", TypeName = "ضريبه جدول", Description = "ضريبة الجدول – نوعية" },
            new Tax { MainCode = "T_4", TypeCode = "W001", SubCode = "W001", TypeName = "الخصم تحت حساب ضريبه", Description = "المقاولات" },
            new Tax { MainCode = "T_4", TypeCode = "W002", SubCode = "W002", TypeName = "الخصم تحت حساب ضريبه", Description = "التوريدات" },
            new Tax { MainCode = "T_4", TypeCode = "W003", SubCode = "W003", TypeName = "الخصم تحت حساب ضريبه", Description = "المشتريات" },
            new Tax { MainCode = "T_4", TypeCode = "W004", SubCode = "W004", TypeName = "الخصم تحت حساب ضريبه", Description = "الخدمات" },
            new Tax { MainCode = "T_4", TypeCode = "W005", SubCode = "W005", TypeName = "الخصم تحت حساب ضريبه", Description = "الجمعيات التعاونية للنقل" },
            new Tax { MainCode = "T_4", TypeCode = "W006", SubCode = "W006", TypeName = "الخصم تحت حساب ضريبه", Description = "الوكالة والسمسرة" },
            new Tax { MainCode = "T_4", TypeCode = "W007", SubCode = "W007", TypeName = "الخصم تحت حساب ضريبه", Description = "شركات الدخان والاسمنت" },
            new Tax { MainCode = "T_4", TypeCode = "W008", SubCode = "W008", TypeName = "الخصم تحت حساب ضريبه", Description = "شركات البترول والاتصالات" },
            new Tax { MainCode = "T_4", TypeCode = "W009", SubCode = "W009", TypeName = "الخصم تحت حساب ضريبه", Description = "دعم الصادرات" },
            new Tax { MainCode = "T_4", TypeCode = "W010", SubCode = "W010", TypeName = "الخصم تحت حساب ضريبه", Description = "أتعاب مهنية" },
            new Tax { MainCode = "T_4", TypeCode = "W011", SubCode = "W011", TypeName = "الخصم تحت حساب ضريبه", Description = "عمولة وسمسرة" },
            new Tax { MainCode = "T_4", TypeCode = "W012", SubCode = "W012", TypeName = "الخصم تحت حساب ضريبه", Description = "تحصيل المستشفيات من الأطباء" },
            new Tax { MainCode = "T_4", TypeCode = "W013", SubCode = "W013", TypeName = "الخصم تحت حساب ضريبه", Description = "إتاوات" },
            new Tax { MainCode = "T_4", TypeCode = "W014", SubCode = "W014", TypeName = "الخصم تحت حساب ضريبه", Description = "تخليص جمركي" },
            new Tax { MainCode = "T_4", TypeCode = "W015", SubCode = "W015", TypeName = "الخصم تحت حساب ضريبه", Description = "إعفاء" },
            new Tax { MainCode = "T_4", TypeCode = "W016", SubCode = "W016", TypeName = "الخصم تحت حساب ضريبه", Description = "دفعات مقدمة" },
            new Tax { MainCode = "T_5", TypeCode = "ST01", SubCode = "ST01", TypeName = "ضريبه الدمغه", Description = "ضريبة الدمغة – نسبية" },
            new Tax { MainCode = "T_6", TypeCode = "ST02", SubCode = "ST02", TypeName = "ضريبه الدمغه", Description = "ضريبة الدمغة – قطعية" },
            new Tax { MainCode = "T_7", TypeCode = "Ent01", SubCode = "Ent01", TypeName = "ضريبه الملاهى", Description = "ضريبة الملاهي نسبية" },
            new Tax { MainCode = "T_7", TypeCode = "Ent02", SubCode = "Ent02", TypeName = "ضريبه الملاهى", Description = "ضريبة الملاهي قطعية" },
            new Tax { MainCode = "T_8", TypeCode = "RD01", SubCode = "RD01", TypeName = "رسم تنميه الموارد", Description = "رسم تنمية الموارد – نسبية" },
            new Tax { MainCode = "T_8", TypeCode = "RD02", SubCode = "RD02", TypeName = "رسم تنميه الموارد", Description = "رسم تنمية الموارد – قطعية" },
            new Tax { MainCode = "T_9", TypeCode = "RD01", SubCode = "RD01", TypeName = "رسم خدمه", Description = "رسم خدمة – نسبية" },
            new Tax { MainCode = "T_9", TypeCode = "RD02", SubCode = "RD02", TypeName = "رسم خدمه", Description = "رسم خدمة – قطعية" },
            new Tax { MainCode = "T_10", TypeCode = "Mn01", SubCode = "Mn01", TypeName = "رسم محليات", Description = "رسم المحليات – نسبة" },
            new Tax { MainCode = "T_10", TypeCode = "Mn02", SubCode = "Mn02", TypeName = "رسم محليات", Description = "رسم المحليات – قطعية" },
            new Tax { MainCode = "T_11", TypeCode = "MI01", SubCode = "MI01", TypeName = "رسم التامين الصحى", Description = "رسم التأمين الصحي – نسبة" },
            new Tax { MainCode = "T_11", TypeCode = "MI02", SubCode = "MI02", TypeName = "رسم التامين الصحى", Description = "رسم التأمين الصحي – قطعية" },
            new Tax { MainCode = "T_12", TypeCode = "OF01", SubCode = "OF01", TypeName = "رسوم اخرى", Description = "رسوم أخرى – نسبة" },
            new Tax { MainCode = "T_12", TypeCode = "OF02", SubCode = "OF02", TypeName = "رسوم اخرى", Description = "رسوم أخرى – قطعية" },
            new Tax { MainCode = "T_13", TypeCode = "ST03", SubCode = "ST03", TypeName = "ضريبه دمغه", Description = "ضريبة الدمغة نسبية (غير ضريبي)" },
            new Tax { MainCode = "T_14", TypeCode = "ST04", SubCode = "ST04", TypeName = "ضريبه دمغه", Description = "ضريبة الدمغة قطعية (غير ضريبي)" },
            new Tax { MainCode = "T_15", TypeCode = "Ent03", SubCode = "Ent03", TypeName = "ضريبه ملاهى", Description = "ضريبة الملاهي نسبية" },
            new Tax { MainCode = "T_15", TypeCode = "Ent04", SubCode = "Ent04", TypeName = "ضريبه ملاهى", Description = "ضريبة الملاهي قطعية" },
            new Tax { MainCode = "T_16", TypeCode = "RD03", SubCode = "RD03", TypeName = "رسوم تنميه الموارد", Description = "رسم تنمية الموارد – نسبة" },
            new Tax { MainCode = "T_16", TypeCode = "RD04", SubCode = "RD04", TypeName = "رسوم تنميه الموارد", Description = "رسم تنمية الموارد – قطعية" },
            new Tax { MainCode = "T_17", TypeCode = "SC03", SubCode = "SC03", TypeName = "رسوم خدمه", Description = "رسم خدمة – نسبة" },
            new Tax { MainCode = "T_17", TypeCode = "SC04", SubCode = "SC04", TypeName = "رسوم خدمه", Description = "رسم خدمة – قطعية" },
            new Tax { MainCode = "T_18", TypeCode = "Mn03", SubCode = "Mn03", TypeName = "رسم المحليات", Description = "رسم المحليات – نسبة" },
            new Tax { MainCode = "T_18", TypeCode = "Mn04", SubCode = "Mn04", TypeName = "رسم المحليات", Description = "رسم المحليات – قطعية" },
            new Tax { MainCode = "T_19", TypeCode = "MI03", SubCode = "MI03", TypeName = "رسال التامين الصحى", Description = "رسم التأمين الصحي – نسبة" },
            new Tax { MainCode = "T_19", TypeCode = "MI04", SubCode = "MI04", TypeName = "رسوم التامين الصحى", Description = "رسم التأمين الصحي – قطعية" },
            new Tax { MainCode = "T_20", TypeCode = "OF03", SubCode = "OF03", TypeName = "رسوم أخرى", Description = "رسوم أخرى – نسبة" },
            new Tax { MainCode = "T_20", TypeCode = "OF04", SubCode = "OF04", TypeName = "رسوم أخرى", Description = "رسوم أخرى – قطعية" },
        };

        foreach (var tax in taxes)
        {
            await _repository.CreateAsync(tax);
        }

        await _unitOfWork.SaveAsync();
    }
}


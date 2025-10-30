using Application.Common.Extensions;
using Application.Common.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.SalesOrderManager;

public class SalesOrderService
{
    private readonly ICommandRepository<SalesOrder> _salesOrderRepository;
    private readonly ICommandRepository<SalesOrderItem> _salesOrderItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SalesOrderService(
        ICommandRepository<SalesOrder> salesOrderRepository,
        ICommandRepository<SalesOrderItem> salesOrderItemRepository,
        IUnitOfWork unitOfWork
        )
    {
        _salesOrderRepository = salesOrderRepository;
        _salesOrderItemRepository = salesOrderItemRepository;
        _unitOfWork = unitOfWork;
    }

    public void Recalculate(string salesOrderId)
    {
        var salesOrder = _salesOrderRepository.GetQuery()
            .Include(so => so.SalesOrderItemList.Where(i => !i.IsDeleted))
                .ThenInclude(item => item.Product)
                    .ThenInclude(p => p.Vat)
            .Include(so => so.Tax)
            .FirstOrDefault(x => x.Id == salesOrderId);

        if (salesOrder == null) return;

        double subtotal = 0;
        double vatAmount = 0;

        foreach (var item in salesOrder.SalesOrderItemList)
        {
            double lineTotal = (item.Quantity ?? 0) * (item.UnitPrice ?? 0);
            subtotal += lineTotal;

            if (item.Product?.Vat?.Percentage != null)
            {
                vatAmount += lineTotal * (item.Product.Vat.Percentage.Value / 100);
            }
        }

        double baseForWithholding = subtotal + vatAmount;
        double withholdingAmount = 0;

        if (salesOrder.Tax?.Percentage > 0)
        {
            withholdingAmount = baseForWithholding * (salesOrder.Tax.Percentage.Value / 100);
        }
        // else: TaxId null → withholding = 0

        double discount = salesOrder.Discount ?? 0;
        double total = baseForWithholding - withholdingAmount - discount;

        salesOrder.BeforeTaxAmount = subtotal;
        salesOrder.VatAmount = vatAmount;
        salesOrder.WithholdingAmount = withholdingAmount;
        salesOrder.AfterTaxAmount = total;

        _unitOfWork.Save();
    }

    //public void Recalculate2(string salesOrderId)
    //{
    //    var salesOrder = _salesOrderRepository.GetQuery()
    //        .Include(so => so.SalesOrderItemList)
    //        .Include(so => so.SalesOrderTaxes)
    //            .ThenInclude(st => st.Tax)
    //        .FirstOrDefault(so => so.Id == salesOrderId);

    //    if (salesOrder == null) return;

    //    // Calculate subtotal
    //    double? subtotal = salesOrder.SalesOrderItemList.Sum(item => item.Total);

    //    // Calculate tax from multiple tax rates
    //    decimal totalTax = 0;
    //    foreach (var salesOrderTax in salesOrder.SalesOrderTaxes)
    //    {
    //        totalTax += subtotal * (salesOrderTax.Tax.Rate / 100m);
    //    }

    //    // Update sales order totals
    //    salesOrder.BeforeTaxAmount = subtotal;
    //    salesOrder.TaxAmount = totalTax;
    //    salesOrder.AfterTaxAmount = subtotal + totalTax;

    //    _salesOrderRepository.Update(salesOrder);
    //    _unitOfWork.Save();
    //}
}

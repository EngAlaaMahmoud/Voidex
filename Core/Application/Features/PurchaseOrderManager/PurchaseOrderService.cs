using Application.Common.Extensions;
using Application.Common.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.PurchaseOrderManager;

public class PurchaseOrderService
{
    private readonly ICommandRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly ICommandRepository<PurchaseOrderItem> _purchaseOrderItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrderService(
        ICommandRepository<PurchaseOrder> purchaseOrderRepository,
        ICommandRepository<PurchaseOrderItem> purchaseOrderItemRepository,
        IUnitOfWork unitOfWork
        )
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseOrderItemRepository = purchaseOrderItemRepository;
        _unitOfWork = unitOfWork;
    }


    public void Recalculate(string purchaseOrderId)
    {
        var purchaseOrder = _purchaseOrderRepository.GetQuery()
            .Include(po => po.PurchaseOrderItemList.Where(i => !i.IsDeleted))
                .ThenInclude(item => item.Product)
                    .ThenInclude(p => p.Vat)
            .Include(po => po.Tax)
            .FirstOrDefault(x => x.Id == purchaseOrderId);

        if (purchaseOrder == null) return;

        double subtotal = 0;
        double vatAmount = 0;

        foreach (var item in purchaseOrder.PurchaseOrderItemList)
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

        if (purchaseOrder.Tax?.Percentage > 0)
        {
            withholdingAmount = baseForWithholding * (purchaseOrder.Tax.Percentage.Value / 100);
        }

        double discount = purchaseOrder.Discount ?? 0;
        double total = baseForWithholding - withholdingAmount - discount;

        purchaseOrder.BeforeTaxAmount = subtotal;
        purchaseOrder.VatAmount = vatAmount;
        purchaseOrder.WithholdingAmount = withholdingAmount;
        purchaseOrder.AfterTaxAmount = total;

        _purchaseOrderRepository.Update(purchaseOrder);
        _unitOfWork.Save();
    }
    // csharp
    //public void Recalculate(string purchaseOrderId)
    //{
    //    var purchaseOrder = _purchaseOrderRepository.GetQuery()
    //        .Include(po => po.PurchaseOrderItemList)
    //            .ThenInclude(item => item.Product)
    //                //.ThenInclude(p => p.Tax)
    //        .Include(po => po.PurchaseOrderItemList)
    //            .ThenInclude(item => item.Product)
    //                .ThenInclude(p => p.Vat)
    //        .FirstOrDefault(x => x.Id == purchaseOrderId);

    //    if (purchaseOrder == null) return;

    //    // Use decimal for precise financial calculations
    //    decimal subtotal = 0;
    //    decimal totalTax = 0;
    //    decimal totalVat = 0;

    //    foreach (var item in purchaseOrder.PurchaseOrderItemList ?? new List<PurchaseOrderItem>())
    //    {
    //        // Convert double? to decimal for calculations
    //        decimal quantity = (decimal)(item.Quantity ?? 0);
    //        decimal unitPrice = (decimal)(item.UnitPrice ?? 0);

    //        decimal itemTotal = quantity * unitPrice;
    //        subtotal += itemTotal;

    //        // Calculate tax from product
    //        //if (item.Product?.Tax?.Percentage != null)
    //        //{
    //        //    decimal taxPercentage = (decimal)item.Product.Tax.Percentage.Value;
    //        //    totalTax += itemTotal * (taxPercentage / 100m);
    //        //}

    //        // Calculate VAT from product
    //        if (item.Product?.Vat?.Percentage != null)
    //        {
    //            decimal vatPercentage = (decimal)item.Product.Vat.Percentage.Value;
    //            totalVat += itemTotal * (vatPercentage / 100m);
    //        }
    //    }

    //    purchaseOrder.BeforeTaxAmount = (double)subtotal;
    //    purchaseOrder.TaxAmount = (double)(totalTax + totalVat);
    //    purchaseOrder.AfterTaxAmount = (double)(subtotal + totalTax + totalVat);

    //    _purchaseOrderRepository.Update(purchaseOrder);
    //    _unitOfWork.Save();
    //}

    //public void Recalculate(string purchaseOrderId)
    //{
    //    var purchaseOrder = _purchaseOrderRepository
    //        .GetQuery()
    //        .ApplyIsDeletedFilter()
    //        .Where(x => x.Id == purchaseOrderId)
    //        .Include(x => x.PurchaseOrderTaxes)
    //        .ThenInclude(a=>a.Tax)
    //        .SingleOrDefault();

    //    if (purchaseOrder == null)
    //        return;

    //    var purchaseOrderItems = _purchaseOrderItemRepository
    //        .GetQuery()
    //        .ApplyIsDeletedFilter()
    //        .Where(x => x.PurchaseOrderId == purchaseOrderId)
    //        .ToList();

    //    purchaseOrder.BeforeTaxAmount = purchaseOrderItems.Sum(x => x.Total ?? 0);

    //    var taxPercentage = purchaseOrder.Tax?.Percentage ?? 0;
    //    purchaseOrder.TaxAmount = (purchaseOrder.BeforeTaxAmount ?? 0) * taxPercentage / 100;

    //    purchaseOrder.AfterTaxAmount = (purchaseOrder.BeforeTaxAmount ?? 0) + (purchaseOrder.TaxAmount ?? 0);

    //    _purchaseOrderRepository.Update(purchaseOrder);
    //    _unitOfWork.Save();
    //}
}

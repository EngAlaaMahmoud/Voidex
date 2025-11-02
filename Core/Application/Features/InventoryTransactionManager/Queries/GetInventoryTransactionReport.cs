using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.InventoryTransactionManager.Queries
{
    public record InventoryTransactionReportDto
    {
        public DateTime? TransactionDate { get; init; }
        public string? Description { get; init; }
        public double? PurchasePrice { get; init; }
        public double? SalesPrice { get; init; }
        public double? Incoming { get; init; }
        public double? Outgoing { get; init; }
        public double? Stock { get; init; }
        public double? PurchaseValue { get; init; }
        public double? SalesValue { get; init; }
        public string? WarehouseName { get; init; }
        public string? ProductName { get; init; }
        public string? ProductNumber { get; init; }
        public string? ModuleName { get; init; }
        public string? ModuleNumber { get; init; }
        public string? StatusName { get; init; }
        public DateTime? MovementDate { get; init; }
        public DateTime? CreatedAtUtc { get; init; }
    }

    public class GetInventoryTransactionReportResult
    {
        public List<InventoryTransactionReportDto>? Data { get; init; }
    }

    public class GetInventoryTransactionReportRequest : IRequest<GetInventoryTransactionReportResult>
    {
        public string? WarehouseId { get; init; }
        public string? ProductId { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public bool IsDeleted { get; init; } = false;
    }

    public class GetInventoryTransactionReportHandler : IRequestHandler<GetInventoryTransactionReportRequest, GetInventoryTransactionReportResult>
    {
        private readonly IMapper _mapper;
        private readonly IQueryContext _context;

        public GetInventoryTransactionReportHandler(IMapper mapper, IQueryContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<GetInventoryTransactionReportResult> Handle(GetInventoryTransactionReportRequest request, CancellationToken cancellationToken)
        {
            var baseQuery = _context
                .InventoryTransaction
                .AsNoTracking()
                .ApplyIsDeletedFilter(request.IsDeleted)
                .Include(x => x.Warehouse)
                .Include(x => x.Product)
                .Include(x => x.WarehouseFrom)
                .Include(x => x.WarehouseTo)
                .Where(x =>
                    x.Product!.Physical == true &&
                    x.Warehouse!.SystemWarehouse == false &&
                    x.Status == Domain.Enums.InventoryTransactionStatus.Confirmed
                );

            if (!string.IsNullOrEmpty(request.WarehouseId))
            {
                baseQuery = baseQuery.Where(x => x.WarehouseId == request.WarehouseId);
            }

            if (!string.IsNullOrEmpty(request.ProductId))
            {
                baseQuery = baseQuery.Where(x => x.ProductId == request.ProductId);
            }

            if (request.FromDate.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.MovementDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.MovementDate <= request.ToDate.Value);
            }

            var orderedTransactions = await baseQuery
                .OrderBy(x => x.MovementDate)
                .ThenBy(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            var reportData = CalculateRunningBalances(orderedTransactions);

            return new GetInventoryTransactionReportResult
            {
                Data = reportData
            };
        }

        private List<InventoryTransactionReportDto> CalculateRunningBalances(List<InventoryTransaction> transactions)
        {
            var groupedTransactions = transactions
                .GroupBy(x => new { x.WarehouseId, x.ProductId })
                .SelectMany(group =>
                {
                    var productTransactions = new List<InventoryTransactionReportDto>();
                    double productRunningStock = 0;
                    double productRunningPurchaseValue = 0;
                    double productRunningSalesValue = 0;

                    foreach (var transaction in group.OrderBy(x => x.MovementDate).ThenBy(x => x.CreatedAtUtc))
                    {
                        // quantity moved (absolute)
                        var qty = transaction.Movement ?? 0.0;

                        // movement sign for running stock (positive for incoming, negative for outgoing)
                        double movement = 0.0;
                        double incoming = 0.0;
                        double outgoing = 0.0;

                        switch (transaction.TransType)
                        {
                            case InventoryTransType.In:
                                movement = qty;
                                incoming = qty;
                                break;
                            case InventoryTransType.Out:
                                movement = -qty;
                                outgoing = qty;
                                break;
                            default:
                                // fallback: if WarehouseFromId present treat as outgoing, else incoming
                                if (!string.IsNullOrEmpty(transaction.WarehouseFromId))
                                {
                                    movement = -qty;
                                    outgoing = qty;
                                }
                                else
                                {
                                    movement = qty;
                                    incoming = qty;
                                }
                                break;
                        }

                        // Determine unit prices (use Product.UnitPrice when no transaction-specific price available)
                        double unitPurchasePrice = transaction.Product?.UnitPrice ?? 0.0;
                        double unitSalesPrice = transaction.Product?.UnitPrice ?? 0.0;

                        // Compute values using absolute qty and unit prices
                        double purchaseValue = unitPurchasePrice * movement;
                        double salesValue = unitSalesPrice * movement;

                        // Update running balances
                        productRunningStock += movement;
                        productRunningPurchaseValue += purchaseValue;
                        productRunningSalesValue += salesValue;

                        var dto = new InventoryTransactionReportDto
                        {
                            TransactionDate = transaction.MovementDate,
                            Description = transaction.ModuleName, // if you have a better description use it
                            PurchasePrice = unitPurchasePrice != 0 ? unitPurchasePrice : (double?)null,
                            SalesPrice = unitSalesPrice != 0 ? unitSalesPrice : (double?)null,
                            Incoming = incoming > 0 ? incoming : (double?)null,
                            Outgoing = outgoing > 0 ? outgoing : (double?)null,
                            Stock = productRunningStock,
                            PurchaseValue = productRunningPurchaseValue,
                            SalesValue = productRunningSalesValue,
                            WarehouseName = transaction.Warehouse?.Name,
                            ProductName = transaction.Product?.Name,
                            ProductNumber = transaction.Product?.Number,
                            ModuleName = transaction.ModuleName,
                            ModuleNumber = transaction.ModuleNumber,
                            StatusName = transaction.Status.ToString(),
                            MovementDate = transaction.MovementDate,
                            CreatedAtUtc = transaction.CreatedAtUtc
                        };

                        productTransactions.Add(dto);
                    }

                    return productTransactions;
                })
                .ToList();

            return groupedTransactions;
        }
    }
}
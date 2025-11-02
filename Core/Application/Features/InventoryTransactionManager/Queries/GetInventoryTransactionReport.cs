using Application.Common.CQS.Queries;
using Application.Common.Extensions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
            // Group by warehouse + product and compute running balances per group
            var grouped = transactions
                .GroupBy(x => new { x.WarehouseId, x.ProductId })
                .SelectMany(g =>
                {
                    var list = new List<InventoryTransactionReportDto>();
                    double productRunningStock = 0;
                    double productRunningPurchaseValue = 0;
                    double productRunningSalesValue = 0;

                    foreach (var tx in g.OrderBy(x => x.MovementDate).ThenBy(x => x.CreatedAtUtc))
                    {
                        var qty = tx.Movement ?? 0.0;
                        double movement = 0.0;
                        double incoming = 0.0;
                        double outgoing = 0.0;

                        if (tx.TransType == InventoryTransType.In)
                        {
                            movement = qty;
                            incoming = qty;
                        }
                        else if (tx.TransType == InventoryTransType.Out)
                        {
                            movement = -qty;
                            outgoing = qty;
                        }
                        else
                        {
                            // Fallback: use Movement sign if TransType not set
                            movement = qty;
                            if (qty > 0) incoming = qty;
                            else { outgoing = Math.Abs(qty); movement = qty; }
                        }

                        // There are no explicit PurchasePrice/SalesPrice fields on InventoryTransaction in this repo,
                        // so keep price-based values null / zero.
                        double purchaseValue = 0.0;
                        double salesValue = 0.0;

                        productRunningStock += movement;
                        productRunningPurchaseValue += purchaseValue;
                        productRunningSalesValue += salesValue;

                        var dto = new InventoryTransactionReportDto
                        {
                            TransactionDate = tx.MovementDate,
                            Description = null,
                            PurchasePrice = null,
                            SalesPrice = null,
                            Incoming = incoming > 0 ? incoming : (double?)null,
                            Outgoing = outgoing > 0 ? outgoing : (double?)null,
                            Stock = productRunningStock,
                            PurchaseValue = productRunningPurchaseValue != 0 ? productRunningPurchaseValue : null,
                            SalesValue = productRunningSalesValue != 0 ? productRunningSalesValue : null,
                            WarehouseName = tx.Warehouse?.Name,
                            ProductName = tx.Product?.Name,
                            ProductNumber = tx.Product?.Number,
                            ModuleName = tx.ModuleName,
                            ModuleNumber = tx.ModuleNumber,
                            StatusName = tx.Status.ToString(),
                            MovementDate = tx.MovementDate,
                            CreatedAtUtc = tx.CreatedAtUtc
                        };

                        list.Add(dto);
                    }

                    return list;
                })
                .ToList();

            return grouped;
        }
    }
}
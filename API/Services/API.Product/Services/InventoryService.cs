using API.Product.Constants;
using API.Product.Interfaces;
using API.Product.Models.Commands;
using API.Product.Models.DTOs;
using API.Product.Models.Queries;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Commons.Models;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Product.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IUnitOfWork uow, ILogger<InventoryService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    // Lịch sử giao dịch
    public async Task<PagedResult<InventoryTransactionDto>> GetTransactionsAsync(
        InventoryQuery query, CancellationToken ct = default)
    {
        var q = _uow.InventoryTransactions.Query()
            .Include(t => t.Product)
            .AsQueryable();

        if (query.ProductId.HasValue)
            q = q.Where(t => t.ProductId == (decimal)query.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(query.TransactionType))
            q = q.Where(t => t.TransactionType == query.TransactionType.ToUpper());

        if (query.FromDate.HasValue)
            q = q.Where(t => t.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            q = q.Where(t => t.CreatedAt <= query.ToDate.Value);

        q = q.OrderByDescending(t => t.CreatedAt);

        var totalCount = await q.CountAsync(ct);
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new InventoryTransactionDto
            {
                Id = (long)t.Id,
                ProductId = (long)t.ProductId,
                ProductName = t.Product != null ? t.Product.Name : null,
                TransactionType = t.TransactionType,
                Quantity = (int)t.Quantity,
                UnitCost = t.UnitCost,
                ReferenceId = t.ReferenceId.HasValue ? (long)t.ReferenceId.Value : null,
                ReferenceType = t.ReferenceType,
                Note = t.Note,
                CreatedBy = t.CreatedBy.HasValue ? (long)t.CreatedBy.Value : null,
                CreatedAt = t.CreatedAt,
            })
            .ToListAsync(ct);

        return new PagedResult<InventoryTransactionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // Nhập hàng
    public async Task ImportStockAsync(StockImportRequest request, CancellationToken ct = default)
    {
        var product = await _uow.Products.FirstOrDefaultAsync(
            p => p.Id == (decimal)request.ProductId, ct)
            ?? throw new NotFoundException("Sản phẩm", request.ProductId);

        var tx = new InventoryTransaction
        {
            ProductId = (decimal)request.ProductId,
            TransactionType = ProductConstants.TransactionType.Import,
            Quantity = (int)request.Quantity,
            UnitCost = request.UnitCost,
            Note = request.Note,
            CreatedBy = request.CreatedBy.HasValue ? (decimal)request.CreatedBy.Value : null,
            CreatedAt = DateTime.UtcNow,
        };

        await _uow.InventoryTransactions.AddAsync(tx, ct);

        product.StockQuantity += (decimal)request.Quantity;
        product.UpdatedAt = DateTime.UtcNow;
        _uow.Products.Update(product);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Nhập hàng: ProductId={ProductId}, Qty={Qty}",
            request.ProductId, request.Quantity);
    }

    // Sắp hết hàng
    public async Task<List<LowStockProductDto>> GetLowStockAsync(
        int threshold = 10, CancellationToken ct = default)
    {
        var items = await _uow.Products.Query()
            .Where(p => p.StockQuantity <= (decimal)threshold && p.Status == true)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new LowStockProductDto
            {
                Id = (long)p.Id,
                ProductCode = p.ProductCode,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                StockQuantity = (int)p.StockQuantity,
                SoldQuantity = (int)p.SoldQuantity,
            })
            .ToListAsync(ct);

        return items;
    }

    // Xuất hàng (qua Message Queue) - idempotent theo orderCode
    public async Task ExportStockAsync(string orderCode, List<netcore.Commons.Messages.Events.OrderItemEventDto> items, CancellationToken ct = default)
    {
        var exportNote = $"Đơn hàng {orderCode} (MQ)";

        var alreadyExported = await _uow.InventoryTransactions.Query()
            .AnyAsync(tx => tx.TransactionType == "SALE" && tx.Note == exportNote, ct);

        if (alreadyExported)
        {
            _logger.LogInformation("Bỏ qua xuất kho {OrderCode}: đã xuất trước đó", orderCode);
            return;
        }

        foreach (var item in items)
        {
            var product = await _uow.Products.FirstOrDefaultAsync(p => p.Id == (decimal)item.ProductId, ct);
            if (product == null)
            {
                throw new NotFoundException("Sản phẩm", item.ProductId);
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new MessageException($"'{product.Name}' không đủ hàng (còn {product.StockQuantity}).");
            }

            var tx = new InventoryTransaction
            {
                ProductId = (decimal)item.ProductId,
                TransactionType = "SALE",
                Quantity = -(int)item.Quantity,
                UnitCost = product.SalePrice,
                ReferenceType = "ORDER",
                ReferenceId = null,
                Note = exportNote,
                CreatedAt = DateTime.UtcNow,
            };
            await _uow.InventoryTransactions.AddAsync(tx, ct);

            product.StockQuantity -= (decimal)item.Quantity;
            product.SoldQuantity += (decimal)item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            _uow.Products.Update(product);
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Đã xuất kho cho đơn hàng {OrderCode} qua Message Queue", orderCode);
    }

    // Hoàn kho (Saga compensation) - idempotent theo orderCode
    public async Task ReleaseStockAsync(string orderCode, List<netcore.Commons.Messages.Events.OrderItemEventDto> items, CancellationToken ct = default)
    {
        var releaseNote = $"Hoàn kho {orderCode}";

        var alreadyReleased = await _uow.InventoryTransactions.Query()
            .AnyAsync(tx => tx.TransactionType == "RELEASE" && tx.Note == releaseNote, ct);

        if (alreadyReleased)
        {
            _logger.LogInformation("Bỏ qua hoàn kho {OrderCode}: đã release trước đó", orderCode);
            return;
        }

        foreach (var item in items)
        {
            var product = await _uow.Products.FirstOrDefaultAsync(p => p.Id == (decimal)item.ProductId, ct);
            if (product == null)
            {
                _logger.LogWarning("Không tìm thấy product {ProductId} khi hoàn kho {OrderCode}",
                    item.ProductId, orderCode);
                continue;
            }

            var tx = new InventoryTransaction
            {
                ProductId = (decimal)item.ProductId,
                TransactionType = "RELEASE",
                Quantity = (int)item.Quantity,
                UnitCost = product.SalePrice,
                ReferenceType = "ORDER",
                Note = releaseNote,
                CreatedAt = DateTime.UtcNow,
            };
            await _uow.InventoryTransactions.AddAsync(tx, ct);

            product.StockQuantity += (decimal)item.Quantity;
            if (product.SoldQuantity >= (decimal)item.Quantity)
            {
                product.SoldQuantity -= (decimal)item.Quantity;
            }
            product.UpdatedAt = DateTime.UtcNow;
            _uow.Products.Update(product);
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Đã hoàn kho cho đơn hàng {OrderCode}", orderCode);
    }
}

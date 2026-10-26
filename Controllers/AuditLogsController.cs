using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.Infrastructure;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

// Admin-only, read-only. Human-friendly labels are looked up AFTER paging;
// stored audit IDs and immutable log records are never modified.
[Authorize(Roles = "Admin")]
public sealed class AuditLogsController : Controller
{
    private readonly ApplicationDbContext _db;
    public AuditLogsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(string? actionFilter, int page = 1,
        CancellationToken ct = default)
    {
        const int pageSize = 30;
        page = Math.Clamp(page, 1, 10000);
        var filter = (actionFilter ?? "").Trim();
        if (filter.Length > 80) filter = filter[..80];
        var query = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (filter.Length > 0)
            query = query.Where(x => x.ActionCode.StartsWith(filter));

        var count = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.AuditLogId)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new AuditLogRowVm
            {
                AuditLogId = x.AuditLogId,
                OccurredAtUtc = x.OccurredAtUtc,
                ActorUserId = x.ActorUserId,
                BusinessId = x.BusinessId,
                ActionCode = x.ActionCode,
                EntityType = x.EntityType,
                EntityId = x.EntityId ?? ""
            }).ToListAsync(ct);

        // Resolve a bounded page of foreign keys, never an N+1 query per row.
        static Guid[] Keys(IEnumerable<AuditLogRowVm> items, string type) =>
            items.Where(r => string.Equals(r.EntityType, type, StringComparison.OrdinalIgnoreCase))
                .Select(r => Guid.TryParse(r.EntityId, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty).Distinct().ToArray();

        var actorIds = rows.Where(r => r.ActorUserId.HasValue)
            .Select(r => r.ActorUserId!.Value);
        var userIds = actorIds.Concat(Keys(rows, "User"))
            .Concat(Keys(rows, "ApplicationUser")).Distinct().ToArray();
        var userNames = await AdminDisplayLookup.UsersAsync(_db, userIds, ct);

        var businessIds = rows.Where(r => r.BusinessId.HasValue)
            .Select(r => r.BusinessId!.Value)
            .Concat(Keys(rows, "Business")).Distinct().ToArray();
        var businessNames = await AdminDisplayLookup.BusinessesAsync(_db, businessIds, ct);

        var documentIds = Keys(rows, "Document");
        var documents = await _db.Documents.AsNoTracking()
            .Where(d => documentIds.Contains(d.DocumentId))
            .Select(d => new { d.DocumentId, d.OriginalFileName })
            .ToDictionaryAsync(d => d.DocumentId, d => d.OriginalFileName, ct);

        var invoiceIds = Keys(rows, "Invoice");
        var invoices = await _db.Invoices.AsNoTracking()
            .Where(i => invoiceIds.Contains(i.InvoiceId))
            .Select(i => new { i.InvoiceId, i.InvoiceNumber })
            .ToDictionaryAsync(i => i.InvoiceId, i => i.InvoiceNumber, ct);

        var customerIds = Keys(rows, "Customer");
        var customers = await _db.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.CustomerId))
            .Select(c => new { c.CustomerId, c.DisplayName })
            .ToDictionaryAsync(c => c.CustomerId, c => c.DisplayName, ct);

        var sectorIds = Keys(rows, "Sector");
        var sectors = await _db.Sectors.AsNoTracking()
            .Where(s => sectorIds.Contains(s.SectorId))
            .Select(s => new { s.SectorId, s.Name })
            .ToDictionaryAsync(s => s.SectorId, s => s.Name, ct);

        var referenceIds = Keys(rows, "ReferenceValue");
        var references = await _db.ReferenceValues.AsNoTracking()
            .Where(v => referenceIds.Contains(v.ReferenceValueId))
            .Select(v => new { v.ReferenceValueId, v.Name })
            .ToDictionaryAsync(v => v.ReferenceValueId, v => v.Name, ct);

        var privacyIds = Keys(rows, "DataSubjectRequest");
        var requests = await _db.DataSubjectRequests.AsNoTracking()
            .Where(r => privacyIds.Contains(r.DataSubjectRequestId))
            .Select(r => new { r.DataSubjectRequestId, r.RequestType })
            .ToDictionaryAsync(r => r.DataSubjectRequestId, r => r.RequestType, ct);

        foreach (var row in rows)
        {
            row.ActionDescription = AdminDisplayLookup.ActionLabel(row.ActionCode);

            if (row.ActorUserId is Guid actorId)
            {
                if (userNames.TryGetValue(actorId, out var actor))
                {
                    row.ActorName = actor.Name;
                    row.ActorEmail = actor.Email;
                }
                else row.ActorName = "Account unavailable";
            }
            else row.ActorName = "System";

            row.BusinessName = row.BusinessId is null ? "Platform-wide" :
                businessNames.TryGetValue(row.BusinessId.Value, out var name)
                    ? name : "Business no longer available";

            var entityType = row.EntityType;
            var hasId = Guid.TryParse(row.EntityId, out var id);
            var label = AdminDisplayLookup.EntityTypeLabel(entityType);
            string? resolved = null;

            if (hasId)
            {
                resolved = entityType switch
                {
                    "Business" when businessNames.TryGetValue(id, out var business) => business,
                    "User" or "ApplicationUser" when userNames.TryGetValue(id, out var user) => user.Name,
                    "Document" when documents.TryGetValue(id, out var document) => document,
                    "Invoice" when invoices.TryGetValue(id, out var invoice) => invoice,
                    "Customer" when customers.TryGetValue(id, out var customer) => customer,
                    "Sector" when sectors.TryGetValue(id, out var sector) => sector,
                    "ReferenceValue" when references.TryGetValue(id, out var reference) => reference,
                    "DataSubjectRequest" when requests.TryGetValue(id, out var request) => request + " request",
                    _ => null
                };
            }
            row.EntityDescription = string.IsNullOrWhiteSpace(resolved)
                ? label + " record" : resolved;
        }

        return View(new AuditLogsIndexVm
        {
            ActionFilter = filter, Page = page, TotalRows = count,
            Rows = rows, PageSize = pageSize
        });
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.Models;

namespace SparkVenture.Controllers;

// Internal helper, NOT an MVC endpoint. Existing controllers and DbContext stay untouched.
public abstract class FinanceControllerBase : Controller
{
    protected readonly ApplicationDbContext Db;
    protected readonly UserManager<ApplicationUser> Users;
    protected FinanceControllerBase(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        Db = db;
        Users = users;
    }

    protected sealed class FinanceAccess
    {
        public string BusinessName { get; set; } = "";
        public string Role { get; set; } = "";
        public bool CanWrite => Role is "Owner" or "Manager" or "Accountant";
    }

    protected bool CurrentUserId(out Guid userId) =>
        Guid.TryParse(Users.GetUserId(User), out userId);

    protected Task<FinanceAccess?> AccessAsync(Guid businessId, Guid userId, CancellationToken ct) =>
        (from m in Db.BusinessMembers.AsNoTracking()
         join b in Db.Businesses.AsNoTracking() on m.BusinessId equals b.BusinessId
         where businessId != Guid.Empty && m.BusinessId == businessId && m.UserId == userId &&
               m.MembershipStatus == "Active"
         select new FinanceAccess { BusinessName = b.TradingName ?? "Business", Role = m.BusinessRole })
        .FirstOrDefaultAsync(ct);

    // Only a single active membership is automatically selected. Never use selection as authorisation.
    protected async Task<Guid?> DefaultBusinessAsync(Guid userId, CancellationToken ct)
    {
        var ids = await Db.BusinessMembers.AsNoTracking()
            .Where(m => m.UserId == userId && m.MembershipStatus == "Active")
            .Select(m => m.BusinessId).Distinct().Take(2).ToListAsync(ct);
        return ids.Count == 1 ? ids[0] : null;
    }

    protected void FinanceAudit(Guid userId, Guid businessId, string action,
        string entityType, Guid entityId, string payload = "{}")
    {
        Db.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(), ActorUserId = userId, BusinessId = businessId,
            ActionCode = action, EntityType = entityType, EntityId = entityId.ToString(),
            PayloadJson = payload, OccurredAtUtc = DateTime.UtcNow
        });
    }
}

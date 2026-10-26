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

// Abstract helper only. IMPORTANT: DO NOT add [NonController] here: that attribute
// is inherited and would disable all derived controllers (the Phase C routing bug).
public abstract class OpportunityControllerBase : Controller
{
    protected readonly ApplicationDbContext Db;
    protected readonly UserManager<ApplicationUser> Users;
    protected OpportunityControllerBase(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        Db = db;
        Users = users;
    }
    protected sealed class AccessInfo
    {
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public bool CanManage => Role is "Owner" or "Manager";
    }
    protected bool TryUser(out Guid userId) => Guid.TryParse(Users.GetUserId(User), out userId);
    protected Task<AccessInfo?> AccessAsync(Guid businessId, Guid userId, CancellationToken ct) =>
        (from membership in Db.BusinessMembers.AsNoTracking()
         join business in Db.Businesses.AsNoTracking() on membership.BusinessId equals business.BusinessId
         where businessId != Guid.Empty && membership.BusinessId == businessId &&
               membership.UserId == userId && membership.MembershipStatus == "Active"
         select new AccessInfo { Name = business.TradingName ?? "Business", Role = membership.BusinessRole })
        .FirstOrDefaultAsync(ct);
    protected async Task<Guid?> SingleBusinessAsync(Guid userId, CancellationToken ct)
    {
        var ids = await Db.BusinessMembers.AsNoTracking()
            .Where(x => x.UserId == userId && x.MembershipStatus == "Active")
            .Select(x => x.BusinessId).Distinct().Take(2).ToListAsync(ct);
        return ids.Count == 1 ? ids[0] : null;
    }
    protected IActionResult ChooseBusiness() => RedirectToAction("Index", "Businesses");
    protected void Audit(Guid userId, Guid businessId, string action, string entityType, Guid entityId)
    {
        Db.AuditLogs.Add(new AuditLog {
            AuditLogId = Guid.NewGuid(), ActorUserId = userId, BusinessId = businessId,
            OccurredAtUtc = DateTime.UtcNow, ActionCode = action, EntityType = entityType,
            EntityId = entityId.ToString(), PayloadJson = "{}"
        });
    }
}

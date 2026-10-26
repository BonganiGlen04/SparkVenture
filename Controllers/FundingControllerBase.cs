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

// Abstract C# base class, not an MVC endpoint. Avoid the inherited non-controller attribute.
public abstract class FundingControllerBase : Controller
{
    protected readonly ApplicationDbContext Db;
    protected readonly UserManager<ApplicationUser> Users;

    protected FundingControllerBase(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        Db = db;
        Users = users;
    }

    protected sealed class BusinessAccess
    {
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public bool CanManage => Role == "Owner" || Role == "Manager";
    }

    protected bool CurrentUser(out Guid userId) => Guid.TryParse(Users.GetUserId(User), out userId);

    protected Task<BusinessAccess?> GetAccessAsync(Guid businessId, Guid userId, CancellationToken ct) =>
        (from member in Db.BusinessMembers.AsNoTracking()
         join business in Db.Businesses.AsNoTracking() on member.BusinessId equals business.BusinessId
         where businessId != Guid.Empty && member.BusinessId == businessId && member.UserId == userId &&
               member.MembershipStatus == "Active"
         select new BusinessAccess
         {
             Name = business.TradingName ?? "Business",
             Role = member.BusinessRole
         }).FirstOrDefaultAsync(ct);

    protected async Task<Guid?> GetSingleBusinessIdAsync(Guid userId, CancellationToken ct)
    {
        var ids = await Db.BusinessMembers.AsNoTracking()
            .Where(x => x.UserId == userId && x.MembershipStatus == "Active")
            .Select(x => x.BusinessId).Distinct().Take(2).ToListAsync(ct);
        return ids.Count == 1 ? ids[0] : null;
    }

    protected IActionResult PickBusiness() => RedirectToAction("Index", "Businesses");

    protected void AddAudit(Guid userId, Guid businessId, string action, string entityType, Guid entityId)
    {
        Db.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(), ActorUserId = userId, BusinessId = businessId,
            OccurredAtUtc = DateTime.UtcNow, ActionCode = action, EntityType = entityType,
            EntityId = entityId.ToString(), PayloadJson = "{}"
        });
    }
}

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;

namespace SparkVenture.Infrastructure;

/// <summary>
/// Remembers WHICH business the entrepreneur is viewing for navigation only.
/// This is not an authorization mechanism. Each business controller must continue
/// to check membership/permissions on every requested business record.
/// </summary>
public sealed class ActiveBusinessContextMiddleware
{
    public const string BusinessIdItemKey = "SparkVenture.ActiveBusinessId";
    public const string BusinessNameItemKey = "SparkVenture.ActiveBusinessName";
    private const string CookieName = "sparkventure.active-business";
    private readonly RequestDelegate _next;

    public ActiveBusinessContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        // The admin workspace is separate and must not acquire an entrepreneur context.
        if (context.User.Identity?.IsAuthenticated != true || context.User.IsInRole("Admin") ||
            !Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            await _next(context);
            return;
        }

        var requested = RequestedBusinessId(context);
        Guid? selectedId = null;
        string? selectedName = null;

        if (requested.HasValue)
        {
            // An explicit business selection overrides the old one only when authorized.
            var selected = await (from m in db.BusinessMembers.AsNoTracking()
                                  join b in db.Businesses.AsNoTracking()
                                      on m.BusinessId equals b.BusinessId
                                  where m.UserId == userId && m.MembershipStatus == "Active" &&
                                        m.BusinessId == requested.Value
                                  select new { b.BusinessId, b.TradingName })
                                  .FirstOrDefaultAsync(context.RequestAborted);
            if (selected != null)
            {
                selectedId = selected.BusinessId;
                selectedName = selected.TradingName;
            }
            // If an explicit ID was supplied but is invalid/unauthorized, do NOT
            // silently substitute another business for this request.
        }
        else
        {
            var fromCookie = ReadCookie(context.Request.Cookies[CookieName], userId);
            if (fromCookie.HasValue)
            {
                var selected = await (from m in db.BusinessMembers.AsNoTracking()
                                      join b in db.Businesses.AsNoTracking()
                                          on m.BusinessId equals b.BusinessId
                                      where m.UserId == userId && m.MembershipStatus == "Active" &&
                                            m.BusinessId == fromCookie.Value
                                      select new { b.BusinessId, b.TradingName })
                                      .FirstOrDefaultAsync(context.RequestAborted);
                if (selected != null)
                {
                    selectedId = selected.BusinessId;
                    selectedName = selected.TradingName;
                }
            }

            if (!selectedId.HasValue)
            {
                // Single-business users need no selector; multi-business users
                // must choose explicitly when nothing has been remembered.
                var choices = await (from m in db.BusinessMembers.AsNoTracking()
                                     join b in db.Businesses.AsNoTracking()
                                         on m.BusinessId equals b.BusinessId
                                     where m.UserId == userId && m.MembershipStatus == "Active"
                                     select new { b.BusinessId, b.TradingName })
                                     .Distinct()
                                     .Take(2)
                                     .ToListAsync(context.RequestAborted);
                if (choices.Count == 1)
                {
                    selectedId = choices[0].BusinessId;
                    selectedName = choices[0].TradingName;
                }
            }
        }

        if (selectedId.HasValue)
        {
            context.Items[BusinessIdItemKey] = selectedId.Value;
            context.Items[BusinessNameItemKey] = selectedName ?? "Selected business";

            // Navigation preference only. A forged cookie cannot grant access;
            // we validate membership above and all destination actions authorize.
            context.Response.Cookies.Append(CookieName,
                $"{userId:N}:{selectedId.Value:N}",
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(14),
                    Path = "/"
                });
        }
        else if (requested is null)
        {
            // Clear stale selection (user changed accounts, lost membership, etc.).
            context.Response.Cookies.Delete(CookieName);
        }

        await _next(context);
    }

    private static Guid? RequestedBusinessId(HttpContext context)
    {
        if (Guid.TryParse(context.Request.Query["businessId"].ToString(), out var id))
            return id;
        if (Guid.TryParse(context.Request.RouteValues["businessId"]?.ToString(), out id))
            return id;

        // 'id' is a business GUID ONLY on Businesses/Details and Businesses/Edit.
        // On Invoices/Details, Customers/Edit, etc. 'id' refers to a different entity.
        var controller = context.Request.RouteValues["controller"]?.ToString();
        var action = context.Request.RouteValues["action"]?.ToString();
        if (string.Equals(controller, "Businesses", StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(action, "Details", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(action, "Edit", StringComparison.OrdinalIgnoreCase)))
        {
            if (Guid.TryParse(context.Request.Query["id"].ToString(), out id)) return id;
            if (Guid.TryParse(context.Request.RouteValues["id"]?.ToString(), out id)) return id;
        }
        return null;
    }

    private static Guid? ReadCookie(string? raw, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var parts = raw.Split(':');
        if (parts.Length != 2 ||
            !Guid.TryParse(parts[0], out var cookieUser) ||
            !Guid.TryParse(parts[1], out var cookieBusiness) ||
            cookieUser != userId || cookieBusiness == Guid.Empty) return null;
        return cookieBusiness;
    }
}

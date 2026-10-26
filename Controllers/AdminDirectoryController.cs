using System;
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

// Minimal, read-only staff directory. Does not grant roles, expose passwords,
// or provide customer impersonation. These endpoints must remain Admin-only.
[Authorize(Roles = "Admin")]
public sealed class AdminDirectoryController : Controller
{
    private const int PageSize = 25;
    private readonly ApplicationDbContext _db;

    public AdminDirectoryController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Users(string? search, int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Clamp(page, 1, 10000);
        var term = (search ?? "").Trim();
        if (term.Length > 100) term = term[..100];

        var query = _db.Users.AsNoTracking().AsQueryable();
        if (term.Length > 0)
            query = query.Where(u => (u.Email != null && u.Email.Contains(term)) ||
                                     (u.UserName != null && u.UserName.Contains(term)));

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(u => u.Email).ThenBy(u => u.Id)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .Select(u => new AdminUserRowVm
            {
                UserId = u.Id,
                Email = u.Email ?? u.UserName ?? "",
                EmailConfirmed = u.EmailConfirmed
            }).ToListAsync(cancellationToken);

        var ids = rows.Select(r => r.UserId).ToArray();
        var users = await AdminDisplayLookup.UsersAsync(_db, ids, cancellationToken);
        var roleRows = await (from link in _db.UserRoles.AsNoTracking()
                              join role in _db.Roles.AsNoTracking() on link.RoleId equals role.Id
                              where ids.Contains(link.UserId)
                              select new { link.UserId, role.Name })
            .ToListAsync(cancellationToken);
        var roleLookup = roleRows.GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => string.Join(", ",
                g.Select(r => r.Name).Where(n => !string.IsNullOrEmpty(n))));
        foreach (var row in rows)
        {
            row.DisplayName = users.TryGetValue(row.UserId, out var account)
                ? account.Name : "Account unavailable";
            row.Roles = roleLookup.TryGetValue(row.UserId, out var names)
                ? names : "No role assigned";
        }

        return View(new AdminUsersIndexVm
        {
            Search = term, Page = page, PageSize = PageSize,
            Total = total, Rows = rows
        });
    }

    [HttpGet]
    public async Task<IActionResult> Businesses(string? search, int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Clamp(page, 1, 10000);
        var term = (search ?? "").Trim();
        if (term.Length > 100) term = term[..100];

        var query = _db.Businesses.AsNoTracking().AsQueryable();
        if (term.Length > 0)
            query = query.Where(b => b.TradingName.Contains(term));

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(b => b.TradingName).ThenBy(b => b.BusinessId)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .Select(b => new AdminBusinessRowVm
            {
                BusinessId = b.BusinessId,
                TradingName = b.TradingName,
                RegistrationNumber = b.RegistrationNumber ?? "",
                ActiveMembers = _db.BusinessMembers.Count(m => m.BusinessId == b.BusinessId && m.MembershipStatus == "Active")
            }).ToListAsync(cancellationToken);

        return View(new AdminBusinessesIndexVm
        {
            Search = term, Page = page, PageSize = PageSize,
            Total = total, Rows = rows
        });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;

namespace SparkVenture.Infrastructure;

// Presentation-only lookups for staff pages. GUIDs stay in database relations,
// audit records and form values; they are not human-facing labels.
public static class AdminDisplayLookup
{
    public sealed record UserLabel(string Name, string Email);

    public static async Task<Dictionary<Guid, UserLabel>> UsersAsync(
        ApplicationDbContext db, IEnumerable<Guid> ids, CancellationToken ct)
    {
        var keys = ids.Distinct().ToArray();
        if (keys.Length == 0) return new Dictionary<Guid, UserLabel>();

        var users = await db.Users.AsNoTracking()
            .Where(u => keys.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.UserName })
            .ToListAsync(ct);

        var profiles = await db.UserProfiles.AsNoTracking()
            .Where(p => keys.Contains(p.UserId))
            .Select(p => new { p.UserId, p.FirstName, p.LastName })
            .ToListAsync(ct);

        var profileMap = profiles.GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => g.First());

        return users.ToDictionary(u => u.Id, u =>
        {
            var email = u.Email ?? u.UserName ?? "";
            var name = "";
            if (profileMap.TryGetValue(u.Id, out var profile))
                name = string.Join(" ", new[] { profile.FirstName, profile.LastName }
                    .Where(part => !string.IsNullOrWhiteSpace(part))
                    .Select(part => part.Trim()));
            return new UserLabel(
                string.IsNullOrWhiteSpace(name) ? (string.IsNullOrWhiteSpace(email) ? "Unnamed account" : email) : name,
                email);
        });
    }

    public static async Task<Dictionary<Guid, string>> BusinessesAsync(
        ApplicationDbContext db, IEnumerable<Guid> ids, CancellationToken ct)
    {
        var keys = ids.Distinct().ToArray();
        if (keys.Length == 0) return new Dictionary<Guid, string>();
        return await db.Businesses.AsNoTracking()
            .Where(b => keys.Contains(b.BusinessId))
            .Select(b => new { b.BusinessId, b.TradingName, b.LegalName })
            .ToDictionaryAsync(b => b.BusinessId,
                b => b.TradingName ?? b.LegalName ?? "Unnamed business", ct);
    }

    public static string ActionLabel(string code) => code switch
    {
        "business.created" => "Created a business",
        "invoice.issued" => "Issued an invoice",
        "document.uploaded" => "Uploaded a document",
        "document.downloaded" => "Downloaded a document",
        "document.business_link_removed" => "Removed a document link",
        "registration.status-changed" => "Changed registration status",
        "auth.login-succeeded" => "Signed in",
        "admin.reference-updated" => "Updated a reference value",
        "admin.sector-updated" => "Updated a business sector",
        "admin.privacy-notice-published" => "Published a privacy notice",
        "privacy.request-review-started" => "Started reviewing a privacy request",
        "privacy.request-received" => "Received a privacy request",
        "privacy.marketing-consent-changed" => "Updated marketing consent",
        _ => FriendlyWords(code)
    };

    // Fallback for new action/entity codes: readable without silently inventing meaning.
    private static string FriendlyWords(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Activity";
        var cleaned = value.Replace('.', ' ').Replace('_', ' ').Replace('-', ' ').Trim();
        return char.ToUpperInvariant(cleaned[0]) + cleaned[1..];
    }

    public static string EntityTypeLabel(string? type) => type switch
    {
        "DataSubjectRequest" => "Privacy request",
        "RegistrationApplication" => "Registration application",
        "RegistrationApplicationDocument" => "Registration document link",
        "BusinessMember" => "Business membership",
        "ConsentEvent" => "Consent record",
        "PaymentTransaction" => "Payment",
        "ReferenceValue" => "Reference value",
        "FundingApplication" => "Funding application",
        "TenderApplication" => "Tender application",
        "ApplicationUser" or "User" => "Account",
        _ => FriendlyWords(type)
    };
}

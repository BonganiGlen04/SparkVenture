using System;
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.Models;

namespace SparkVenture.Infrastructure;

// Simple, server-side audit metadata only. Never log personal request details or sensitive payloads.
public static class GovernanceAudit
{
    public static void Add(ApplicationDbContext db, Guid actorId, string action, string entityType, Guid entityId)
    {
        db.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            ActorUserId = actorId,
            BusinessId = null,
            OccurredAtUtc = DateTime.UtcNow,
            ActionCode = action,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            PayloadJson = "{}"
        });
    }
}

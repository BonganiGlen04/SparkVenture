using System;
using System.Collections.Generic;

namespace SparkVenture.ViewModels;

public sealed class NotificationsIndexViewModel
{
    public int UnreadCount { get; set; }
    public List<NotificationRowViewModel> Items { get; set; } = new();
}

public sealed class NotificationRowViewModel
{
    public Guid NotificationId { get; set; }
    public Guid? BusinessId { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Channel { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}

public sealed class NotificationPreferencesViewModel
{
    public List<NotificationPreferenceRowViewModel> Items { get; set; } = new();
}

public sealed class NotificationPreferenceRowViewModel
{
    public Guid NotificationPreferenceId { get; set; }
    public string Channel { get; set; } = "";
    public string Category { get; set; } = "";
    public bool IsEnabled { get; set; }
}

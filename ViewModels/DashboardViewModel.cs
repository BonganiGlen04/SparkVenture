namespace SparkVenture.ViewModels;
 
public class DashboardViewModel
{
    /// <summary>The signed-in user's email address.</summary>
    public string Email { get; set; } = "";
}
 
/// <summary>Used by the NotificationBadge view component.</summary>
public record NotificationBadgeViewModel(int Count, bool DotOnly);
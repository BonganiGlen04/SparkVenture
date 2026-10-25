namespace SparkVenture.ViewModels;

public sealed class NotificationBadgeViewModel
{
    public int Count { get; }
    public bool DotOnly { get; }
    public NotificationBadgeViewModel(int count, bool dotOnly)
    {
        Count = count;
        DotOnly = dotOnly;
    }
}

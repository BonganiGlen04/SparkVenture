
namespace SparkVenture.ViewModels
{
    public class NotificationBadgeViewModel
    {
        public int Count { get; set; }

        public bool DotOnly { get; set; }

        public NotificationBadgeViewModel(int count, bool dotOnly)
        {
            Count = count;
            DotOnly = dotOnly;
        }
    }
}

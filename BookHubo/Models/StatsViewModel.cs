namespace BookHubo.Models
{
    public class StatsViewModel
    {
        public int TotalActiveListings { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}

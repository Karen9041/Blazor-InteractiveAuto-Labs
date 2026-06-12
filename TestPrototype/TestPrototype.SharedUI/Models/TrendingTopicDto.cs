namespace TestPrototype.SharedUI.Models
{
    public class TrendingTopicDto
    {
        public int Rank { get; set; }
        public string? Category { get; set; }
        public string? TopicName { get; set; }
        public int DiscussionCount { get; set; }
        public bool IsNew { get; set; }
    }
}

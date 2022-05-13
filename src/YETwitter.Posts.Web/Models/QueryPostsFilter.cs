#nullable enable
namespace YETwitter.Posts.Web.Models
{
    public class QueryPostsFilter
    {
        public string? Username { get; set; }
        public string? HashTag { get; set; }
        public string? SearchTerm { get; set; }
        public int? PagesSkip { get; set; }
    }
}

namespace YETwitter.Posts.Web.Models
{
    public class CreatePostDto
    {
        public string Rawtext { get; protected set; }

        public string Author { get; protected set; }

        public CreatePostDto(string rawText, string author)
        {
            Rawtext = rawText;
            Author = author;
        }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace YETwitter.Posts.Web.Data.Entities
{
    [Table("posts")]
    public class Post
    {
        [Column("id"), Key]
        public Guid? Id { get; protected set; }

        [Column("create_time", TypeName = "datetime"), Required]
        public DateTime CreateTime { get; protected set; }

        [Column("text", TypeName = "nvarchar(140)"), Required, MaxLength(140)]
        public string Text { get; protected set; }

        [Column("username", TypeName = "varchar(100)"), Required, MaxLength(100)]
        public string Author { get; protected set; }

        public virtual List<HashTag> Hashtags { get; set; } = new List<HashTag>();

        public virtual List<Appeal> Appeals { get; set; } = new List<Appeal>();

        public Post([Required] string text, [Required] string author)
        {
            this.CreateTime = DateTime.UtcNow;
            this.Text = text;
            this.Author = author;
        }

        public Post(Guid id, [Required] string text, [Required] string author)
            : this(text, author)
        {
            this.Id = id;
        }

        protected Post() { }
    }
}

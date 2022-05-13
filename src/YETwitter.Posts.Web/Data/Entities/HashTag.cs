using System.ComponentModel.DataAnnotations.Schema;

namespace YETwitter.Posts.Web.Data.Entities
{
    [Table("hashtags")]
    public class HashTag
    {
        //[Column("id"), Key]
        //public Guid Id { get; set; }

        [Column("value", TypeName = "nvarchar(50)"), Required]
        public string Value { get; set; }

        [Column("post_id"), Required]
        public Guid PostId { get; set; }

        public Post Post { get; set; }

        public HashTag(string value)
        {
            Value = value.TrimStart('#');
        }
    }
}

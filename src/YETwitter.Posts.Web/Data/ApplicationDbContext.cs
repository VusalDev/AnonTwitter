using Microsoft.EntityFrameworkCore;

using YETwitter.Posts.Web.Data.Entities;

namespace YETwitter.Posts.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<Post> Posts { get; set; }

        public virtual DbSet<HashTag> HashTags { get; set; }

        //public virtual DbSet<Appeal> Appeals { get; set; }
        protected ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            var post = builder.Entity<Post>();
            post.Property(x => x.CreateTime).HasDefaultValueSql("GETUTCDATE()").ValueGeneratedOnAdd();
            post.HasMany(x => x.Hashtags)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .HasConstraintName("FK_hashtags_post_id")
                .OnDelete(DeleteBehavior.Cascade);

            var tag = builder.Entity<HashTag>();
            tag.HasKey(x => new { x.PostId, x.Value });
            //tag.HasIndex(x => x.PostId)
            //    .IsClustered(false)
            //    .HasDatabaseName("IX_hashtags_post_id");

            post.HasMany(x => x.Appeals)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .HasConstraintName("FK_appeals_post_id")
                .OnDelete(DeleteBehavior.Cascade);
            var appeal = builder.Entity<Appeal>();
            appeal.HasKey(x => new { x.PostId, x.Value });
        }

        public virtual void SetModified(object entity)
        {
            this.Entry(entity).State = EntityState.Modified;
        }
    }
}

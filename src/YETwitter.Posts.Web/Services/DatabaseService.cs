using Hellang.Middleware.ProblemDetails;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using YETwitter.Posts.Web.Configuration;
using YETwitter.Posts.Web.Data;
using YETwitter.Posts.Web.Data.Entities;
using YETwitter.Posts.Web.Models;

namespace YETwitter.Posts.Web.Services
{
    public interface IDatabaseService
    {
        Task<List<Post>> QueryPostsAsync(QueryPostsFilter filter, CancellationToken cancellationToken = default);

        Task<Post> GetPostAsync(Guid postId, CancellationToken cancellationToken = default);

        Task<Post> AddOrUpdatePostAsync(Post body, CancellationToken cancellationToken = default);

        Task DeletePostAsync(Guid postId, CancellationToken cancellationToken = default);
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly ApplicationDbContext db;
        private readonly ILogger<DatabaseService> logger;
        private readonly PostServiceOptions options;
        public DatabaseService(ApplicationDbContext db, ILogger<DatabaseService> logger, IOptions<PostServiceOptions> options)
        {
            this.db = db;
            this.logger = logger;
            this.options = options.Value;
        }

        public async Task<Post> AddOrUpdatePostAsync(Post post, CancellationToken cancellationToken = default)
        {
            if (post.Id != default)
            {
                db.Posts.Attach(post);
                db.Entry(post).State = EntityState.Modified;
            }
            else
            {
                db.Posts.Add(post);
            }

            await db.SaveChangesAsync(cancellationToken);
            return post;
        }

        public async Task DeletePostAsync(Guid postId, CancellationToken cancellationToken = default)
        {
            var post = await db.Posts
                .SingleOrDefaultAsync(p => p.Id == postId, cancellationToken: cancellationToken) ?? throw new ProblemDetailsException((int)System.Net.HttpStatusCode.NotFound, "Post not found");
            db.Posts.Remove(post);
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<Post> GetPostAsync(Guid postId, CancellationToken cancellationToken = default)
        {
            return await db.Posts
                .Include(p => p.Hashtags)
                .Include(p => p.Appeals)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == postId, cancellationToken: cancellationToken) ?? throw new ProblemDetailsException((int)System.Net.HttpStatusCode.NotFound, "Post not found");
        }

        public async Task<List<Post>> QueryPostsAsync(QueryPostsFilter filter, CancellationToken cancellationToken = default)
        {
            var query = db.Posts
                .Include(p => p.Hashtags)
                .Include(p => p.Appeals)
                .AsNoTracking();
            if (!string.IsNullOrEmpty(filter.Username))
            {
                query = query.Where(x => x.Appeals.Any(ht => ht.Value.Equals(filter.Username, StringComparison.OrdinalIgnoreCase)));
            }
            if (!string.IsNullOrEmpty(filter.HashTag))
            {
                query = query.Where(x => x.Hashtags.Any(ht => ht.Value.Equals(filter.HashTag, StringComparison.OrdinalIgnoreCase)));
            }
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(x => x.Text.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }
            return await query.Skip(options.PageSize * filter.PagesSkip ?? 0).Take(options.PageSize)
                .ToListAsync(cancellationToken);
        }

    }
}

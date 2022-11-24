using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using YETwitter.Posts.Web.Services;

namespace YETwitter.Posts.Web.Controllers
{
    [Authorize]
    public class PostsController : PostControllerBase
    {
        protected readonly IPostService postService;
        public PostsController(IPostService postService)
        {
            this.postService = postService;
        }

        public override async Task<ActionResult<CreatePostResponseModel>> Create([BindRequired, FromBody] CreatePostModel body, CancellationToken cancellationToken = default)
        {
            var username = this.GetUsername();
            var post = await this.postService.CreateAsync(new Models.CreatePostDto(body.Rawtext, username), cancellationToken);
            return this.Ok(post);
        }

        public override async Task<IActionResult> Delete([BindRequired] Guid id, CancellationToken cancellationToken = default)
        {
            await this.postService.DeleteAsync(id, cancellationToken);
            return this.NoContent();
        }

        public override async Task<ActionResult<Post>> GetById([BindRequired] Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentNullException(nameof(id));

            var posts = await this.postService.GetPostByIdAsync(id, cancellationToken);
            return this.Ok(posts);
        }

        public override async Task<ActionResult<QueryPostResponseModel>> QueryPosts([FromQuery] string query = null, [FromQuery] string userId = null, [Microsoft.AspNetCore.Mvc.FromQuery] string hashTag = null, [FromQuery] int? pageSkip = null, CancellationToken cancellationToken = default)
        {
            //if (new[] { query, userId, hashTag }.All(t => string.IsNullOrEmpty(t))) throw new ArgumentNullException(nameof(query));
            var filter = new Models.QueryPostsFilter
            {
                SearchTerm = query,
                HashTag = hashTag,
                Username = userId,
                PagesSkip = pageSkip,
            };
            var posts = await this.postService.QueryPostsAsync(filter, cancellationToken);
            return this.Ok(posts);
        }

        private string GetUsername() => HttpContext.User?.Identity?.Name ?? throw new InvalidOperationException("User not logged in");
    }
}

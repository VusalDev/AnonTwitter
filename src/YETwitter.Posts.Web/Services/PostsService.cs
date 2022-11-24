using System.Text.RegularExpressions;

using YETwitter.Posts.Web.Controllers;

using PostEntity = YETwitter.Posts.Web.Data.Entities.Post;
using HashTagEntity = YETwitter.Posts.Web.Data.Entities.HashTag;
using AppealEntity = YETwitter.Posts.Web.Data.Entities.Appeal;
using YETwitter.Posts.Web.Models;

namespace YETwitter.Posts.Web.Services
{
    public interface IPostService
    {
        Task<Post> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default);

        Task<QueryPostResponseModel> QueryPostsAsync(QueryPostsFilter filter, CancellationToken cancellationToken = default);

        Task<CreatePostResponseModel> CreateAsync(CreatePostDto createPost, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid postId, CancellationToken cancellationToken = default);
    }

    public class PostsService : IPostService
    {
        protected readonly IDatabaseService databaseService;
        public PostsService(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }

        public async Task<CreatePostResponseModel> CreateAsync(CreatePostDto createPost, CancellationToken cancellationToken = default)
        {
            var hashTags = Regex.Matches(createPost.Rawtext, "#\\w+").Cast<Match>().Select(x => x.Value).ToArray();
            var appeals = Regex.Matches(createPost.Rawtext, "@\\w+").Cast<Match>().Select(x => x.Value).ToArray();
            var postEntity = new PostEntity(createPost.Rawtext, createPost.Author) //NOTE: automapper?
            {
                Hashtags = hashTags.Select(h => new HashTagEntity(h)).ToList(),
                Appeals = appeals.Select(h => new AppealEntity(h)).ToList(),
            };
            var post = await databaseService.AddOrUpdatePostAsync(postEntity);
            return new CreatePostResponseModel { Id = post.Id ?? throw new NullReferenceException(nameof(Post.Id)), };
        }

        public async Task DeleteAsync(Guid postId, CancellationToken cancellationToken = default)
        {
            await this.databaseService.DeletePostAsync(postId, cancellationToken);
        }

        public async Task<Post> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken = default)
        {
            var p = await this.databaseService.GetPostAsync(postId, cancellationToken);
            // TODO automapper
            return new Post
            {
                Id = p.Id ?? throw new NullReferenceException(nameof(Post.Id)),
                Text = p.Text,
                Appeals = p.Appeals.Select(a => a.Value).ToList(),
                Hashtags = p.Hashtags.Select(a => a.Value).ToList(),
            };
        }

        public async Task<QueryPostResponseModel> QueryPostsAsync(QueryPostsFilter filter, CancellationToken cancellationToken = default)
        {
            var posts = (await this.databaseService.QueryPostsAsync(filter, cancellationToken))
                .Select(p => new Post
                {
                    Id = p.Id ?? throw new NullReferenceException(nameof(Post.Id)),
                    Text = p.Text,
                    Appeals = p.Appeals.Select(a => a.Value).ToList(),
                    Hashtags = p.Hashtags.Select(a => a.Value).ToList(),
                }).ToList();
            return new QueryPostResponseModel
            {
                Items = posts,
                CurrentPage = filter.PagesSkip ?? 0,
            };
        }
    }
}

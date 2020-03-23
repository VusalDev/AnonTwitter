using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YETwitter.Domain.Core.Seedwork;

namespace YETwitter.Domain.AggregatesModel.PostsAggregate
{
    public interface IPostRepository : IRepository<Post>
    {
        Post Add(Post post);
        Task<Post> FindByIdAsync(Guid id);
        Task<Post> FindByCreatedUserAsync(Guid createdUserId);
        Task<Post> FindBySubscriberAsync(Guid subsriberId);
    }
}

using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using YETwitter.Domain.AggregatesModel.PostsAggregate;

namespace YETwitter.Domain.Events.Posts
{
    public class PostUnLikedDomainEvent : INotification
    {
        public Guid PostId { get; }
        public PostLike Like { get; }

        public PostUnLikedDomainEvent(Guid postId, PostLike like)
        {
            PostId = postId;
            Like = like;
        }
    }
}

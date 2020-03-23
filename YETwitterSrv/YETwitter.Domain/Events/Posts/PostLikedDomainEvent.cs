using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using YETwitter.Domain.AggregatesModel.PostsAggregate;

namespace YETwitter.Domain.Events.Posts
{
    public class PostLikedDomainEvent : INotification
    {
        public Guid PostId { get; }
        public PostLike Like { get; }

        public PostLikedDomainEvent(Guid postId, PostLike like)
        {
            PostId = postId;
            Like = like;
        }
    }
}

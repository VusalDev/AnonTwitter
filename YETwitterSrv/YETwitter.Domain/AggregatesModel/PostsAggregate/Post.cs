using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YETwitter.Domain.Core.Seedwork;
using YETwitter.Domain.Events.Posts;

namespace YETwitter.Domain.AggregatesModel.PostsAggregate
{

    /// <summary>
    /// Пост - запись определенного пользователя длиной до 140 символов.
    /// Посты видят пользователи, подписанные на пользователя - автора.
    /// </summary>
    public class Post : Entity, IAggregateRoot
    {
        private readonly List<PostLike> _likes;
        
        public string Content { get; }

        public Guid CreatedUserId { get; }

        /// <summary>
        /// Время добавления записи (UTC)
        /// </summary>
        public DateTimeOffset CreateTime { get; }

        public IReadOnlyList<PostLike> Likes => _likes;

        public Post(string content, Guid createdUserId)
        {
            CreateTime = DateTimeOffset.UtcNow;
            CreatedUserId = createdUserId;
            Content = content;
            _likes = new List<PostLike>(0);
        }

        public void Like(Guid likedUserId)
        {
            var like = new PostLike(likedUserId);

            if (!Likes.Contains(like))
            {
                _likes.Add(like);
                AddDomainEvent(new PostLikedDomainEvent(this.Id, like));
            }
        }

        public void UnLike(Guid unLikedUserId)
        {
            var like = new PostLike(unLikedUserId);

            if (Likes.Contains(like))
            {
                _likes.Remove(like);
                AddDomainEvent(new PostUnLikedDomainEvent(this.Id, like));
            }
        }

    }
}

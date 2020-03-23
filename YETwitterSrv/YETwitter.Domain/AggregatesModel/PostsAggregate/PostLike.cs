using System;
using System.Collections.Generic;
using System.Linq;
using YETwitter.Domain.Core.Seedwork;

namespace YETwitter.Domain.AggregatesModel.PostsAggregate
{
    public class PostLike : ValueObject
    {
        public Guid UserId { get; }

        public PostLike(Guid userId)
        {
            UserId = userId != default ? userId : throw new ArgumentNullException(nameof(userId));
        }
        
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return UserId;
        }
    }
}
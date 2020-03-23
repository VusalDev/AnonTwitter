using System;
using System.Collections.Generic;
using System.Linq;
using YETwitter.Domain.Core.Seedwork;

namespace YETwitter.Domain.AggregatesModel.UsersAggregate
{
    /// <summary>
    /// Подписка на другого пользователя
    /// </summary>
    public class UserSubscribe : ValueObject
    {
        public Guid UserId { get; }

        public UserSubscribe(Guid userId)
        {
            UserId = userId != default ? userId : throw new ArgumentNullException(nameof(userId));
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return UserId;
        }
    }
}
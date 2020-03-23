using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using YETwitter.Domain.Core.Seedwork;
using YETwitter.Domain.Events.Users;

namespace YETwitter.Domain.AggregatesModel.UsersAggregate
{
    /// <summary>
    /// Пользователь.
    /// Может создавать посты, лайкать посты чужие, подписываться на других пользователей.
    /// </summary>
    public class User : Entity, IAggregateRoot
    {
        private readonly List<Guid> _postIds;
        private readonly List<UserSubscribe> _subscribes;

        public string Login { get; }
        public string UserName { get; }

        /// <summary>
        /// Свои посты
        /// </summary>
        public IReadOnlyList<Guid> PostsIds => _postIds;

        /// <summary>
        /// Подписки на других пользователей
        /// </summary>        
        public IReadOnlyList<UserSubscribe> Subscribes => _subscribes;


        public User(Guid id, string login, string userName)
        {
            Id = id;
            Login = login;
            UserName = userName;
            _subscribes = new List<UserSubscribe>(0);
            _postIds = new List<Guid>(0);
        }

        public void Subscribe(Guid subscribeToId)
        {
            var subscribe = new UserSubscribe(subscribeToId);

            if (!_subscribes.Contains(subscribe))
            {
                _subscribes.Add(subscribe);
                AddDomainEvent(new UserSubscribedDomainEvent(this.Id, subscribe));
            }
        }

        public void UnSubscribe(Guid unSubscribeFromId)
        {
            var subscribe = new UserSubscribe(unSubscribeFromId);

            if (_subscribes.Contains(subscribe))
            {
                _subscribes.Remove(subscribe);
                AddDomainEvent(new UserUnSubscribedDomainEvent(this.Id, subscribe));
            }
        }
    }
}

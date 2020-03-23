using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using YETwitter.Domain.AggregatesModel.UsersAggregate;

namespace YETwitter.Domain.Events.Users
{
    public class UserSubscribedDomainEvent : INotification
    {
        public Guid InvokedUserId { get; }
        public UserSubscribe Subscribe { get; }

        public UserSubscribedDomainEvent(Guid invokedUserId, UserSubscribe subscribe)
        {
            InvokedUserId = invokedUserId;
            Subscribe = subscribe;
        }
    }
}

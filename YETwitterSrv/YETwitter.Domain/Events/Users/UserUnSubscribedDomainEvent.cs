using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using YETwitter.Domain.AggregatesModel.UsersAggregate;

namespace YETwitter.Domain.Events.Users
{
    public class UserUnSubscribedDomainEvent : INotification
    {
        public Guid InvokedUserId { get; }
        public UserSubscribe Subscribe { get; }

        public UserUnSubscribedDomainEvent(Guid invokedUserId, UserSubscribe subscribe)
        {
            InvokedUserId = invokedUserId;
            Subscribe = subscribe;
        }
    }
}

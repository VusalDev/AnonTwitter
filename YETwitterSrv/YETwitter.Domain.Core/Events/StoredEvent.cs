using System;
using System.Collections.Generic;
using System.Text;

namespace YETwitter.Domain.Core.Events
{
    public class StoredEvent : Event
    {
        public StoredEvent(Event theEvent, string data, Guid userId)
        {
            Id = Guid.NewGuid();
            AggregateId = theEvent.AggregateId;
            MessageType = theEvent.MessageType;
            Data = data;
            UserId = userId;
        }

        // EF Constructor
        protected StoredEvent() { }

        public Guid Id { get; private set; }

        public string Data { get; private set; }

        public Guid UserId { get; private set; }
    }
}
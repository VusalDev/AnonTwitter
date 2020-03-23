using YETwitter.Domain.Core.Events;
using System;
using System.ComponentModel.DataAnnotations;

namespace YETwitter.Domain.Core.Commands
{
    public abstract class Command : Message
    {
        public DateTime Timestamp { get; private set; }
        public ValidationResult ValidationResult { get; set; }

        protected Command()
        {
            Timestamp = DateTime.UtcNow;
        }

        public abstract bool IsValid();
    }
}

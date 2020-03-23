using System;
using System.Collections.Generic;
using System.Text;

namespace YETwitter.Domain.Core.Seedwork
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
    }
}

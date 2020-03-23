using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YETwitter.Domain.Core.Seedwork;

namespace YETwitter.Domain.AggregatesModel.UsersAggregate
{
    public interface IUsersRepository : IRepository<User>
    {
        User Add(User user);
        User Update(User user);
        Task<User> FindByIdAsync(Guid id);
        Task<User> FindByLoginAsync(string login);
    }
}

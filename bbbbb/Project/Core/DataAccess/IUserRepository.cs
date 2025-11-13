using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.DataAccess
{
    public interface IUserRepository
    {
        Task AddAsync(ToDoUser user, CancellationToken cancellationToken);
        Task<ToDoUser?> GetUserAsync(Guid id, CancellationToken cancellationToken);
        Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken);
    }
}


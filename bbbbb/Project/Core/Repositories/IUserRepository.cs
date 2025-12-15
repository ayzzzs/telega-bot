using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Repositories
{
    public interface IUserRepository
    {
        Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken cancellationToken);
        Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken);
        Task AddAsync(ToDoUser user, CancellationToken cancellationToken);
    }
}


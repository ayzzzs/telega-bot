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
        Task AddAsync(ToDoUser user, CancellationToken ct);
        Task<ToDoUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct);
    }
}


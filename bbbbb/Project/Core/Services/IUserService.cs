using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Services
{
    public interface IUserService
    {
        Task RegisterAsync(long telegramId, string username, CancellationToken ct);
        Task<ToDoUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct);
    }
}

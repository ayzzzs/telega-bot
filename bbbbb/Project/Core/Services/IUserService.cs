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
        Task<ToDoUser> RegisterAsync(long telegramUserId, string telegramUserName, CancellationToken cancellationToken);
        Task<ToDoUser?> GetByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken);
    }
}

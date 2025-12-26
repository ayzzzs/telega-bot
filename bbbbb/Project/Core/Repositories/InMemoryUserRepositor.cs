using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Repositories
{

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new();

        public Task AddAsync(ToDoUser user, CancellationToken cancellationToken)
        {
            if (!_users.Any(u => u.UserId == user.UserId))
                _users.Add(user);
            return Task.CompletedTask;
        }

        public Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var u = _users.FirstOrDefault(x => x.UserId == userId);
            return Task.FromResult(u);
        }

        public Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken)
        {
            var u = _users.FirstOrDefault(x => x.TelegramUserId == telegramUserId);
            return Task.FromResult(u);
        }
    }
}


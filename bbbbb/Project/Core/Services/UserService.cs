using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Repositories;

namespace bbbbb.Project.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<ToDoUser> RegisterAsync(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            var existing = await _repo.GetUserByTelegramUserIdAsync(telegramUserId, cancellationToken);
            if (existing != null) return existing;

            var user = new ToDoUser(Guid.NewGuid(), telegramUserId, telegramUserName);
            await _repo.AddAsync(user, cancellationToken);
            return user;
        }

        public Task<ToDoUser?> GetByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken)
        {
            return _repo.GetUserByTelegramUserIdAsync(telegramUserId, cancellationToken);
        }
    }
}

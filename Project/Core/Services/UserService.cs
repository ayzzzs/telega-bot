using System;
using System.Threading;
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

        public async Task RegisterAsync(long telegramId, string username, CancellationToken ct)
        {
            var existing = await _repo.GetByTelegramIdAsync(telegramId, ct);
            if (existing != null) return;

            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramId,
                Username = username
            };

            await _repo.AddAsync(user, ct);
        }

        public Task<ToDoUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            return _repo.GetByTelegramIdAsync(telegramId, ct);
        }
    }
}
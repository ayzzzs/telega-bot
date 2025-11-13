using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.DataAccess;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<ToDoUser> GetOrCreateUserAsync(long telegramUserId, string? username, CancellationToken cancellationToken)
        {
            // Ищем пользователя по Telegram ID
            var user = await _repository.GetUserByTelegramUserIdAsync(telegramUserId, cancellationToken);

            if (user == null)
            {
                user = new ToDoUser(Guid.NewGuid(), telegramUserId, username ?? "Unknown");
                await _repository.AddAsync(user, cancellationToken);
            }

            return user;
        }
    }
}

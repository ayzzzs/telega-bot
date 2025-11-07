using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.DataAccess;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Infrastructure.DataAccess
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new();

        public ToDoUser? GetUser(Guid userId) =>
            _users.FirstOrDefault(u => u.UserId == userId);

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId) =>
            _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);

        public void Add(ToDoUser user)
        {
            if (!_users.Any(u => u.UserId == user.UserId))
                _users.Add(user);
        }
    }
}

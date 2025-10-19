using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbbbb
{
    public class UserService : IUserService
    {
        private readonly List<ToDoUser> _users = new();

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var existing = _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
            if (existing != null) return existing;

            var user = new ToDoUser(telegramUserId, telegramUserName);
            _users.Add(user);
            return user;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
        }
    }
}

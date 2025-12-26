using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbbbb.Project.Core.Entities
{

    public class ToDoUser
    {

        public Guid UserId { get; }
        public long TelegramUserId { get; }
        public string TelegramUserName { get; }
        public DateTime RegisteredAt { get; }

        public ToDoUser(Guid userId, long telegramUserId, string telegramUserName)
        {
            UserId = userId;
            TelegramUserId = telegramUserId;
            TelegramUserName = telegramUserName;
            RegisteredAt = DateTime.UtcNow;
        }
    }
}

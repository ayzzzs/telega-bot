using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbbbb
{

    public class ToDoUser
    {
        public long TelegramUserId { get; }
        public string TelegramUserName { get; }
        public Guid UserId { get; }
        public DateTime RegisteredAt { get; }

        public ToDoUser(long telegramUserId, string telegramUserName)
        {
            TelegramUserId = telegramUserId;
            TelegramUserName = telegramUserName;
            UserId = Guid.NewGuid();
            RegisteredAt = DateTime.UtcNow;
        }
    }
}

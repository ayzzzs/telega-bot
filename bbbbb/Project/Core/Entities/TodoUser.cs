using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbbbb.Project.Core.Entities
{

    public class ToDoUser
    {
        public Guid UserId { get; set; }
        public long TelegramId { get; set; }
        public string Username { get; set; }
        public long TelegramUserId { get; internal set; }

        public ToDoUser(long telegramId, string username)
        {
            UserId = Guid.NewGuid();
            TelegramId = telegramId;
            Username = username;
        }
    }

}

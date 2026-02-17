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
        public long TelegramUserId { get; set; }
        public string Username { get; set; }

        public ToDoUser(long telegramUserId, string username)
        {
            UserId = Guid.NewGuid();
            TelegramUserId = telegramUserId;
            Username = username;
        }
    }
}
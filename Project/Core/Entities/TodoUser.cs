using System;

namespace bbbbb.Project.Core.Entities
{
    public class ToDoUser
    {
     

        public Guid UserId { get; set; } = Guid.NewGuid();
        public long TelegramUserId { get; set; }
        public string Username { get; set; }
        public long TelegramId { get; }
    }
}
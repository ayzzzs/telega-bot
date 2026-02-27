using System;

namespace bbbbb.Project.Core.Entities
{
    public class ToDoList
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public ToDoUser User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
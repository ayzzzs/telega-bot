using System;
using System.Collections.Generic;
using System.Text;
using bbbbb.Project.Core.Entities;

namespace ConsoleApp3.Project.Core.Entities
{
    public class ToDoList
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; init; }
        public ToDoUser User { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public ToDoList(ToDoUser user, string name)
        {
            User = user;
            Name = name;
        }
    }
}

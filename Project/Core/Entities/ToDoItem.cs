using System;
using ConsoleApp3.Project.Core.Entities;

namespace bbbbb.Project.Core.Entities
{
    public enum ToDoState
    {
        Active,
        Completed
    }

    public class ToDoItem
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public ToDoUser User { get; init; }
        public string Name { get; init; }
        public ToDoState State { get; private set; } = ToDoState.Active;
        public DateTime Deadline { get; init; }
        public ToDoList? List { get; init; }

        public ToDoItem(ToDoUser user, string name, DateTime deadline, ToDoList? list = null)
        {
            User = user;
            Name = name;
            Deadline = deadline;
            List = list;
        }

        public void MarkCompleted()
        {
            State = ToDoState.Completed;
        }
    }
}
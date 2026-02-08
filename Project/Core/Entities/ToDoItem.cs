using System;

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

        public ToDoItem(ToDoUser user, string name, DateTime deadline)
        {
            User = user;
            Name = name;
            Deadline = deadline;
        }

        public void MarkCompleted()
        {
            State = ToDoState.Completed;
        }
    }
}
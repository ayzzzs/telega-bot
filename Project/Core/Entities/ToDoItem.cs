using System;

namespace bbbbb.Project.Core.Entities
{
    public enum ToDoState
    {
        Active = 0,
        Completed = 1
    }

    public class ToDoItem
    {
        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public ToDoState State { get; set; }
        public DateTime Deadline { get; set; }
        public ToDoList? List { get; set; }

        public void MarkCompleted()
        {
            State = ToDoState.Completed;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ToDoItem(ToDoUser user, string name)
        {
            User = user;
            Name = name;
        }

        public void MarkCompleted()
        {
            State = ToDoState.Completed;
        }
    }
}

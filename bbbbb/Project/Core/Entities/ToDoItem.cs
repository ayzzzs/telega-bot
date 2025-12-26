using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbbbb.Project.Core.Entities
{
    public enum ToDoItemState { Active, Completed }

    public class ToDoItem
    {
        public Guid Id { get; }
        public ToDoUser User { get; }
        public string Name { get; }
        public DateTime CreatedAt { get; }
        public ToDoItemState State { get; private set; }
        public DateTime? StateChangedAt { get; private set; }

        public ToDoItem(ToDoUser user, string name)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Название задачи не может быть пустым", nameof(name));

            Id = Guid.NewGuid();
            User = user;
            Name = name.Trim();
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            StateChangedAt = null;
        }

        public void MarkAsCompleted()
        {
            if (State == ToDoItemState.Completed) return;
            State = ToDoItemState.Completed;
            StateChangedAt = DateTime.UtcNow;
        }
    }
}

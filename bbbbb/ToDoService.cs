using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbbbb
{
    public class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _tasks = new();

        public ToDoItem Add(ToDoUser user, string name)
        {
            var task = new ToDoItem(user, name);
            _tasks.Add(task);
            return task;
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _tasks.Where(t => t.User.UserId == userId).ToList();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _tasks.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
        }

        public void MarkCompleted(Guid id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
                task.MarkAsCompleted();
        }

        public void Delete(Guid id)  // <- реализация метода Delete
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
                _tasks.Remove(task);
        }
    }
}

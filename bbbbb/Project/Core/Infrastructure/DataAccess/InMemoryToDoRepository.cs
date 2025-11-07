using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.DataAccess;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Infrastructure.DataAccess
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _tasks = new();

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId) =>
            _tasks.Where(t => t.User.UserId == userId).ToList();

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) =>
            _tasks.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();

        public ToDoItem? Get(Guid id) =>
            _tasks.FirstOrDefault(t => t.Id == id);

        public void Add(ToDoItem item) => _tasks.Add(item);

        public void Update(ToDoItem item)
        {
            var existing = Get(item.Id);
            if (existing == null) return;
            _tasks.Remove(existing);
            _tasks.Add(item);
        }

        public void Delete(Guid id)
        {
            var item = Get(id);
            if (item != null)
                _tasks.Remove(item);
        }

        public bool ExistsByName(Guid userId, string name) =>
            _tasks.Any(t => t.User.UserId == userId && t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public int CountActive(Guid userId) =>
            _tasks.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate) =>
            _tasks.Where(t => t.User.UserId == userId).Where(predicate).ToList();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Repositories
{
    public class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _tasks = new();

        public Task AddAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            _tasks.Add(item);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var t = _tasks.FirstOrDefault(x => x.Id == id);
            if (t != null) _tasks.Remove(t);
            return Task.CompletedTask;
        }

        public Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            var t = _tasks.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(t);
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var list = _tasks.Where(x => x.User.UserId == userId).ToList();
            return Task.FromResult((IReadOnlyList<ToDoItem>)list);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var list = _tasks.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();
            return Task.FromResult((IReadOnlyList<ToDoItem>)list);
        }

        public Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            var existing = _tasks.FirstOrDefault(x => x.Id == item.Id);
            if (existing != null)
            {
                _tasks.Remove(existing);
                _tasks.Add(item);
            }
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
        {
            var exists = _tasks.Any(x => x.User.UserId == userId && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && x.State == ToDoItemState.Active);
            return Task.FromResult(exists);
        }

        public Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
        {
            var count = _tasks.Count(x => x.User.UserId == userId && x.State == ToDoItemState.Active);
            return Task.FromResult(count);
        }

        public Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var result = _tasks.Where(x => x.User.UserId == userId).Where(predicate).ToList();
            return Task.FromResult((IReadOnlyList<ToDoItem>)result);
        }
    }
}

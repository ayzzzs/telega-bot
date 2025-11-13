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

        public Task AddAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            _tasks.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ToDoItem item, CancellationToken cancellationToken)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == item.Id);
            if (existing != null)
            {
                _tasks.Remove(existing);
                _tasks.Add(item);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var item = _tasks.FirstOrDefault(t => t.Id == id);
            if (item != null)
                _tasks.Remove(item);
            return Task.CompletedTask;
        }

        public Task<ToDoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(task);
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var result = _tasks.Where(t => t.User.UserId == userId).ToList();
            return Task.FromResult((IReadOnlyList<ToDoItem>)result);
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var result = _tasks.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
            return Task.FromResult((IReadOnlyList<ToDoItem>)result);
        }

        public Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken cancellationToken)
        {
            var exists = _tasks.Any(t => t.User.UserId == userId && t.Name == name);
            return Task.FromResult(exists);
        }

        public Task<int> CountActiveAsync(Guid userId, CancellationToken cancellationToken)
        {
            var count = _tasks.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);
            return Task.FromResult(count);
        }

        public Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var result = _tasks.Where(t => t.User.UserId == userId).Where(predicate).ToList();
            return Task.FromResult((IReadOnlyList<ToDoItem>)result);
        }
    }
}

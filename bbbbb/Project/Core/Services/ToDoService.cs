using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Exceptions;
using bbbbb.Project.Core.Repositories;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _repo;
        private readonly int _maxTaskCount = 100; // можно поменять
        private readonly int _maxTaskLength = 200;

        public ToDoService(IToDoRepository repo)
        {
            _repo = repo;
        }

        public async Task<ToDoItem> AddTaskAsync(ToDoUser user, string name, CancellationToken cancellationToken)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Имя задачи пустое", nameof(name));
            if (name.Trim().Length > _maxTaskLength) throw new ArgumentException("Слишком длинное имя задачи", nameof(name));

            var activeCount = await _repo.CountActiveAsync(user.UserId, cancellationToken);
            if (activeCount >= _maxTaskCount) throw new InvalidOperationException($"Достигнут лимит задач {_maxTaskCount}");

            var exists = await _repo.ExistsByNameAsync(user.UserId, name.Trim(), cancellationToken);
            if (exists) throw new InvalidOperationException("Задача с таким именем уже есть");

            var item = new ToDoItem(user, name.Trim());
            await _repo.AddAsync(item, cancellationToken);
            return item;
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveTasksAsync(ToDoUser user, CancellationToken cancellationToken)
        {
            return _repo.GetActiveByUserIdAsync(user.UserId, cancellationToken);
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllTasksAsync(ToDoUser user, CancellationToken cancellationToken)
        {
            return _repo.GetAllByUserIdAsync(user.UserId, cancellationToken);
        }

        public Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken cancellationToken)
        {
            string prefix = (namePrefix ?? "").Trim().ToLower();
            return _repo.FindAsync(user.UserId, t => t.Name.ToLower().StartsWith(prefix), cancellationToken);
        }

        public async Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken)
        {
            var item = await _repo.GetAsync(id, cancellationToken);
            if (item == null) throw new ArgumentException("Задача не найдена");
            item.MarkAsCompleted();
            await _repo.UpdateAsync(item, cancellationToken);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return _repo.DeleteAsync(id, cancellationToken);
        }
    }
}
    



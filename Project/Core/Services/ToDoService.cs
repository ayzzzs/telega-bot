using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Exceptions;
using bbbbb.Project.Core.Repositories;
using ConsoleApp3.Project.Core.Entities;

namespace bbbbb.Project.Core.Services
{
    public class ToDoService : IToDoService
    {
        private const int LIMIT = 10;
        private readonly IToDoRepository _repo;

        public ToDoService(IToDoRepository repo)
        {
            _repo = repo;
        }

        public async Task<ToDoItem> AddTaskAsync(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken ct)
        {
            if (await _repo.ExistsByNameAsync(user.UserId, name, ct))
                throw new DuplicateTaskException(name);

            if (await _repo.CountActiveAsync(user.UserId, ct) >= LIMIT)
                throw new TaskCountLimitException(LIMIT);

            var item = new ToDoItem(user, name, deadline, list);
            await _repo.AddAsync(item, ct);
            return item;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
            => await _repo.DeleteAsync(id, ct);

        public async Task MarkCompletedAsync(Guid id, CancellationToken ct)
        {
            var item = await _repo.GetAsync(id, ct);
            if (item == null) return;

            item.MarkCompleted();
            await _repo.UpdateAsync(item, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllTasksAsync(ToDoUser user, CancellationToken ct)
            => await _repo.GetAllByUserIdAsync(user.UserId, ct);

        public async Task<IReadOnlyList<ToDoItem>> GetActiveTasksAsync(ToDoUser user, CancellationToken ct)
            => await _repo.GetActiveByUserIdAsync(user.UserId, ct);

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string text, CancellationToken ct)
            => await _repo.FindAsync(user.UserId,
                t => t.Name.Contains(text, StringComparison.OrdinalIgnoreCase),
                ct);

        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var all = await _repo.GetAllByUserIdAsync(userId, ct);
            return all.Where(t => t.List?.Id == listId).ToList();
        }
    }
}
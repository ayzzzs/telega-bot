using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Repositories
{
    public interface IToDoRepository
    {
        Task AddAsync(ToDoItem item, CancellationToken ct);
        Task UpdateAsync(ToDoItem item, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
        Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct);
        Task<int> CountActiveAsync(Guid userId, CancellationToken ct);
        Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct);
    }
}


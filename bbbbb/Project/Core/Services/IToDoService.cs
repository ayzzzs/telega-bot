using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Services
{
    public interface IToDoService
    {

        Task<ToDoItem> AddTaskAsync(ToDoUser user, string name, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoItem>> GetActiveTasksAsync(ToDoUser user, CancellationToken cancellationToken);
        Task<ToDoItem?> FindAsync(ToDoUser user, string name, CancellationToken cancellationToken);
    }

}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Exceptions;
using bbbbb.Project.Core.DataAccess;
using bbbbb.Project.Core.Services;

namespace bbbbb.Project.Core.Entities
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _repository;

        public ToDoService(IToDoRepository repository)
        {
            _repository = repository;
        }

        public async Task<ToDoItem> AddTaskAsync(ToDoUser user, string name, CancellationToken cancellationToken)
        {
            if (await _repository.ExistsByNameAsync(user.UserId, name, cancellationToken))
                throw new InvalidOperationException("Задача с таким именем уже существует");

            var item = new ToDoItem(user, name);
            await _repository.AddAsync(item, cancellationToken);
            return item;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveTasksAsync(ToDoUser user, CancellationToken cancellationToken)
        {
            return await _repository.GetActiveByUserIdAsync(user.UserId, cancellationToken);
        }

        public async Task<ToDoItem?> FindAsync(ToDoUser user, string name, CancellationToken cancellationToken)
        {
            var list = await _repository.FindAsync(user.UserId, t => t.Name == name, cancellationToken);
            return list.FirstOrDefault();
        }
    }
}
    



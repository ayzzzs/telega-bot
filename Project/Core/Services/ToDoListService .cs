using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities ;
using bbbbb.Project.Core.Repositories;
using ConsoleApp3.Project.Core.Repositories;
using ConsoleApp3.Project.Core.Services;
using static LinqToDB.Internal.Reflection.Methods;

namespace bbbbb.Project.Core.Services
{
    public class ToDoListService : IToDoListService
    {
        private const int MaxNameLength = 10;
        private readonly IToDoListRepository _repo;

        public ToDoListService(IToDoListRepository repo)
        {
            _repo = repo;
        }

        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            if (name.Length > MaxNameLength)
                throw new ArgumentException($"Название списка не может быть больше {MaxNameLength} символов");

            if (await _repo.ExistsByName(user.UserId, name, ct))
                throw new ArgumentException($"Список с именем '{name}' уже существует");

            var list = new ToDoList
            {
                Id = Guid.NewGuid(),
                Name = name,
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.Add(list, ct);
            return list;
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
            => _repo.Get(id, ct);

        public Task Delete(Guid id, CancellationToken ct)
            => _repo.Delete(id, ct);

        public Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
            => _repo.GetByUserId(userId, ct);
    }
}

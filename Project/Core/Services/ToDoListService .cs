using System;
using System.Collections.Generic;
using System.Text;
using bbbbb.Project.Core.Entities;
using ConsoleApp3.Project.Core.Entities;
using ConsoleApp3.Project.Core.Repositories;

namespace ConsoleApp3.Project.Core.Services
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

            var list = new ToDoList(user, name);
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

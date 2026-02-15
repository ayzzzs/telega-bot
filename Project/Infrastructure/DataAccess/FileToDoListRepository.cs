using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ConsoleApp3.Project.Core.Entities;
using ConsoleApp3.Project.Core.Repositories;

namespace ConsoleApp3.Project.Infrastructure.DataAccess
{
    public class FileToDoListRepository : IToDoListRepository
    {
        private readonly string _basePath;

        public FileToDoListRepository(string basePath)
        {
            _basePath = basePath;
            Directory.CreateDirectory(_basePath);
        }

        private string GetPath(Guid id) => Path.Combine(_basePath, $"{id}.json");

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(list);
            await File.WriteAllTextAsync(GetPath(list.Id), json, ct);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            var path = GetPath(id);
            if (!File.Exists(path)) return null;

            var json = await File.ReadAllTextAsync(path, ct);
            return JsonSerializer.Deserialize<ToDoList>(json);
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var result = new List<ToDoList>();

            foreach (var file in Directory.GetFiles(_basePath, "*.json"))
            {
                var json = await File.ReadAllTextAsync(file, ct);
                var list = JsonSerializer.Deserialize<ToDoList>(json);
                if (list != null && list.User.UserId == userId)
                    result.Add(list);
            }

            return result;
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            var path = GetPath(id);
            if (File.Exists(path))
                File.Delete(path);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var lists = await GetByUserId(userId, ct);
            return lists.Any(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}

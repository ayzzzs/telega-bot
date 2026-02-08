using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Repositories;

namespace bbbbb.Project.Infrastructure.DataAccess
{
    public class FileToDoRepository : IToDoRepository
    {
        private readonly string _basePath;
        private readonly string _indexPath;

        private Dictionary<Guid, Guid> _index = new();

        public FileToDoRepository(string basePath)
        {
            _basePath = basePath;
            _indexPath = Path.Combine(_basePath, "index.json");
            Directory.CreateDirectory(_basePath);
            LoadIndex();
        }

        private void LoadIndex()
        {
            if (File.Exists(_indexPath))
            {
                var json = File.ReadAllText(_indexPath);
                _index = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(json) ?? new();
            }
            else
            {
                RebuildIndex();
            }
        }

        private void SaveIndex()
        {
            var json = JsonSerializer.Serialize(_index);
            File.WriteAllText(_indexPath, json);
        }

        private void RebuildIndex()
        {
            _index.Clear();

            foreach (var dir in Directory.GetDirectories(_basePath))
            {
                var userId = Guid.Parse(Path.GetFileName(dir));
                foreach (var file in Directory.GetFiles(dir, "*.json"))
                {
                    var id = Guid.Parse(Path.GetFileNameWithoutExtension(file));
                    _index[id] = userId;
                }
            }

            SaveIndex();
        }

        private string GetUserDir(Guid userId)
        {
            var path = Path.Combine(_basePath, userId.ToString());
            Directory.CreateDirectory(path);
            return path;
        }

        private string GetPath(Guid userId, Guid id) =>
            Path.Combine(GetUserDir(userId), $"{id}.json");

        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(GetPath(item.User.UserId, item.Id), json, ct);
            _index[item.Id] = item.User.UserId;
            SaveIndex();
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            if (!_index.TryGetValue(id, out var userId))
                return null;

            var path = GetPath(userId, id);
            if (!File.Exists(path)) return null;

            var json = await File.ReadAllTextAsync(path, ct);
            return JsonSerializer.Deserialize<ToDoItem>(json);
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(GetPath(item.User.UserId, item.Id), json, ct);
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            if (_index.TryGetValue(id, out var userId))
            {
                var path = GetPath(userId, id);
                if (File.Exists(path))
                    File.Delete(path);

                _index.Remove(id);
                SaveIndex();
            }
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var dir = GetUserDir(userId);
            var result = new List<ToDoItem>();

            foreach (var file in Directory.GetFiles(dir, "*.json"))
            {
                var json = await File.ReadAllTextAsync(file, ct);
                var item = JsonSerializer.Deserialize<ToDoItem>(json);
                if (item != null) result.Add(item);
            }
            return result;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var all = await GetAllByUserIdAsync(userId, ct);
            return all.Where(x => x.State == ToDoState.Active).ToList();
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            var active = await GetActiveByUserIdAsync(userId, ct);
            return active.Count;
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var all = await GetAllByUserIdAsync(userId, ct);
            return all.Where(predicate).ToList();
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            var all = await GetAllByUserIdAsync(userId, ct);
            return all.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Repositories;


public class FileToDoRepository : IToDoRepository
{
    private readonly string _basePath;
    private readonly string _indexFile;
    private readonly Dictionary<Guid, Guid> _index = new();

    public FileToDoRepository(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);

        _indexFile = Path.Combine(_basePath, "index.json");
        LoadIndex();
    }

    private string UserDir(Guid userId) =>
        Path.Combine(_basePath, userId.ToString());

    private string ItemFile(Guid userId, Guid itemId) =>
        Path.Combine(UserDir(userId), $"{itemId}.json");

    private void LoadIndex()
    {
        if (!File.Exists(_indexFile))
        {
            RebuildIndex();
            return;
        }

        var json = File.ReadAllText(_indexFile);
        var data = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(json);
        if (data != null)
            foreach (var kv in data)
                _index[kv.Key] = kv.Value;
    }

    private void SaveIndex()
    {
        File.WriteAllText(_indexFile,
            JsonSerializer.Serialize(_index, new JsonSerializerOptions { WriteIndented = true }));
    }

    private void RebuildIndex()
    {
        _index.Clear();

        foreach (var dir in Directory.GetDirectories(_basePath))
        {
            var userId = Guid.Parse(Path.GetFileName(dir));
            foreach (var file in Directory.GetFiles(dir, "*.json"))
            {
                var itemId = Guid.Parse(Path.GetFileNameWithoutExtension(file));
                _index[itemId] = userId;
            }
        }
        SaveIndex();
    }

    public async Task AddAsync(ToDoItem item, CancellationToken ct)
    {
        var dir = UserDir(item.User.UserId);
        Directory.CreateDirectory(dir);

        var file = ItemFile(item.User.UserId, item.Id);
        await File.WriteAllTextAsync(file, JsonSerializer.Serialize(item), ct);

        _index[item.Id] = item.User.UserId;
        SaveIndex();
    }

    public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
    {
        var file = ItemFile(item.User.UserId, item.Id);
        await File.WriteAllTextAsync(file, JsonSerializer.Serialize(item), ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        if (!_index.TryGetValue(id, out var userId))
            return Task.CompletedTask;

        var file = ItemFile(userId, id);
        if (File.Exists(file))
            File.Delete(file);

        _index.Remove(id);
        SaveIndex();

        return Task.CompletedTask;
    }

    public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
    {
        if (!_index.TryGetValue(id, out var userId))
            return null;

        var file = ItemFile(userId, id);
        if (!File.Exists(file)) return null;

        var json = await File.ReadAllTextAsync(file, ct);
        return JsonSerializer.Deserialize<ToDoItem>(json);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
    {
        var dir = UserDir(userId);
        if (!Directory.Exists(dir)) return [];

        var list = new List<ToDoItem>();
        foreach (var file in Directory.GetFiles(dir, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file, ct);
            var item = JsonSerializer.Deserialize<ToDoItem>(json);
            if (item != null) list.Add(item);
        }
        return list;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
    {
        var all = await GetAllByUserIdAsync(userId, ct);
        return all.Where(x => x.State == ToDoItemState.Active).ToList();
    }

    public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
    {
        var active = await GetActiveByUserIdAsync(userId, ct);
        return active.Count;
    }

    public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
    {
        var all = await GetAllByUserIdAsync(userId, ct);
        return all.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
    {
        var all = await GetAllByUserIdAsync(userId, ct);
        return all.Where(predicate).ToList();
    }
}

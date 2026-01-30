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
    private readonly string _baseDir;

    public FileToDoRepository(string baseDir)
    {
        _baseDir = baseDir;
        Directory.CreateDirectory(_baseDir);
    }

    private string UserDir(Guid userId)
        => Path.Combine(_baseDir, userId.ToString());

    private string TaskPath(Guid userId, Guid taskId)
        => Path.Combine(UserDir(userId), $"{taskId}.json");

    public async Task AddAsync(ToDoItem item, CancellationToken ct)
    {
        var dir = UserDir(item.User.UserId);
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(item);
        await File.WriteAllTextAsync(TaskPath(item.User.UserId, item.Id), json, ct);
    }

    public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        => await AddAsync(item, ct);

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        foreach (var dir in Directory.GetDirectories(_baseDir))
        {
            var path = Path.Combine(dir, $"{id}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
                return;
            }
        }
    }

    public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
    {
        foreach (var dir in Directory.GetDirectories(_baseDir))
        {
            var path = Path.Combine(dir, $"{id}.json");
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path, ct);
                return JsonSerializer.Deserialize<ToDoItem>(json);
            }
        }
        return null;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
    {
        var dir = UserDir(userId);
        if (!Directory.Exists(dir)) return [];
        var list = new List<ToDoItem>();
        foreach (var file in Directory.GetFiles(dir))
        {
            var json = await File.ReadAllTextAsync(file, ct);
            var item = JsonSerializer.Deserialize<ToDoItem>(json);
            if (item != null) list.Add(item);
        }
        return list;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        => (await GetAllByUserIdAsync(userId, ct))
           .Where(t => t.State == ToDoState.Active).ToList();

    public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        => (await GetActiveByUserIdAsync(userId, ct)).Count;

    public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        => (await GetAllByUserIdAsync(userId, ct))
           .Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        => (await GetAllByUserIdAsync(userId, ct))
           .Where(predicate).ToList();
}
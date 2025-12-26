using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Repositories;

public class FileUserRepository : IUserRepository
{
    private readonly string _basePath;

    public FileUserRepository(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    private string UserFile(Guid userId) =>
        Path.Combine(_basePath, $"{userId}.json");

    public async Task AddAsync(ToDoUser user, CancellationToken ct)
    {
        var file = UserFile(user.UserId);
        await File.WriteAllTextAsync(file, JsonSerializer.Serialize(user), ct);
    }

    public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        var file = UserFile(userId);
        if (!File.Exists(file)) return null;

        var json = await File.ReadAllTextAsync(file, ct);
        return JsonSerializer.Deserialize<ToDoUser>(json);
    }

    public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramId, CancellationToken ct)
    {
        foreach (var file in Directory.GetFiles(_basePath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file, ct);
            var user = JsonSerializer.Deserialize<ToDoUser>(json);
            if (user?.TelegramUserId == telegramId)
                return user;
        }
        return null;
    }
}


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

    public async Task AddAsync(ToDoUser user, CancellationToken ct)
    {
        var path = Path.Combine(_basePath, $"{user.UserId}.json");
        var json = JsonSerializer.Serialize(user);
        await File.WriteAllTextAsync(path, json, ct);
    }

    public async Task<ToDoUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct)
    {
        foreach (var file in Directory.GetFiles(_basePath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file, ct);
            var user = JsonSerializer.Deserialize<ToDoUser>(json);
            if (user?.TelegramId == telegramId)
                return user;
        }
        return null;
    }
}


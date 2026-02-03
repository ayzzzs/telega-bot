using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Repositories;

namespace bbbbb.Project.Infrastructure.DataAccess
{
    public class FileUserRepository : IUserRepository
    {
        private readonly string _basePath;

        public FileUserRepository(string basePath)
        {
            _basePath = basePath;
            Directory.CreateDirectory(_basePath);
        }

        private string GetPath(Guid id) => Path.Combine(_basePath, $"{id}.json");

        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(user);
            await File.WriteAllTextAsync(GetPath(user.UserId), json, ct);
        }

        public async Task<ToDoUser?> GetAsync(Guid userId, CancellationToken ct)
        {
            var path = GetPath(userId);
            if (!File.Exists(path)) return null;

            var json = await File.ReadAllTextAsync(path, ct);
            return JsonSerializer.Deserialize<ToDoUser>(json);
        }

        public async Task<ToDoUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            foreach (var file in Directory.GetFiles(_basePath, "*.json"))
            {
                var json = await File.ReadAllTextAsync(file, ct);
                var user = JsonSerializer.Deserialize<ToDoUser>(json);
                if (user != null && user.TelegramUserId == telegramId)
                    return user;
            }
            return null;
        }
    }
}
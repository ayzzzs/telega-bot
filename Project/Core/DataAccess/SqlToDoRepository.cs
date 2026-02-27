using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Repositories;
using bbbbb.Project.Core.DataAccess.Models;
using LinqToDB.Async;

namespace bbbbb.Project.Infrastructure.DataAccess
{
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(item);
            await dbContext.InsertAsync(model, token: ct);
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await (
                from i in dbContext.ToDoItems
                where i.Id == id
                select i
            )
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .FirstOrDefaultAsync(ct);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(item);
            await dbContext.UpdateAsync(model, token: ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            await dbContext.ToDoItems
                .Where(i => i.Id == id)
                .DeleteAsync(ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await (
                from i in dbContext.ToDoItems
                where i.UserId == userId
                select i
            )
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .ToListAsync(ct);

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await (
                from i in dbContext.ToDoItems
                where i.UserId == userId && i.State == 0
                select i
            )
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .ToListAsync(ct);

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.ToDoItems
                .Where(i => i.UserId == userId && i.State == 0)
                .CountAsync(ct);
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.ToDoItems
                .Where(i => i.UserId == userId && i.Name.ToLower() == name.ToLower())
                .AnyAsync(ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await (
                from i in dbContext.ToDoItems
                where i.UserId == userId
                select i
            )
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .ToListAsync(ct);

            var items = models.Select(ModelMapper.MapFromModel).ToList();
            return items.Where(predicate).ToList();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Repositories;
using bbbbb.Project.Core.DataAccess.Models;
using ConsoleApp3.Project.Core.Repositories;
using LinqToDB.Async;

namespace bbbbb.Project.Infrastructure.DataAccess
{
    public class SqlToDoListRepository : IToDoListRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await (
                from l in dbContext.ToDoLists
                where l.Id == id
                select l
            )
            .LoadWith(l => l.User)
            .FirstOrDefaultAsync(ct);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var models = await (
                from l in dbContext.ToDoLists
                where l.UserId == userId
                select l
            )
            .LoadWith(l => l.User)
            .ToListAsync(ct);

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(list);
            await dbContext.InsertAsync(model, token: ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            await dbContext.ToDoLists
                .Where(l => l.Id == id)
                .DeleteAsync(ct);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            return await dbContext.ToDoLists
                .Where(l => l.UserId == userId && l.Name.ToLower() == name.ToLower())
                .AnyAsync(ct);
        }
    }
}
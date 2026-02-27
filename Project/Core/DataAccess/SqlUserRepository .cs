using System;
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
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(user);
            await dbContext.InsertAsync(model, token: ct);
        }

        public async Task<ToDoUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var query = dbContext.ToDoUsers
                .Where(u => u.TelegramUserId == telegramId);

            var model = await query.FirstOrDefaultAsync(ct);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }
    }
}
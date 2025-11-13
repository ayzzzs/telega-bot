using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Exceptions;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.DataAccess;


namespace bbbbb.Project.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _repository;

        public ToDoReportService(IToDoRepository repository)
        {
            _repository = repository;
        }

        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken token)
        {
            var tasks = await _repository.GetAllByUserIdAsync(userId, token);
            int total = tasks.Count;
            int completed = tasks.Count(t => t.State == Entities.ToDoItemState.Completed);
            int active = tasks.Count(t => t.State == Entities.ToDoItemState.Active);
            return (total, completed, active, DateTime.Now);
        }
    }
}

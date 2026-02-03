using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Exceptions;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Repositories;


namespace bbbbb.Project.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _repo;

        public ToDoReportService(IToDoRepository repo)
        {
            _repo = repo;
        }

        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var all = await _repo.GetAllByUserIdAsync(userId, cancellationToken);
            int total = all.Count;
            int completed = all.Count(t => t.State == ToDoState.Completed);
            int active = total - completed;
            return (total, completed, active, DateTime.UtcNow);
        }
    }

}

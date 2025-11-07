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

        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            var tasks = _repository.GetAllByUserId(userId);
            int total = tasks.Count;
            int completed = tasks.Count(t => t.State == ToDoItemState.Completed);
            int active = total - completed;
            return (total, completed, active, DateTime.Now);
        }
    }
}

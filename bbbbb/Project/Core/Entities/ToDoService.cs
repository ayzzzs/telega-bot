using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Exceptions;
using bbbbb.Project.Core.DataAccess;
using bbbbb.Project.Core.Services;

namespace bbbbb.Project.Core.Entities
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _repository;
        private readonly int _maxTaskCount = 10;
        private readonly int _maxTaskLength = 50;

        public ToDoService(IToDoRepository repository)
        {
            _repository = repository;
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название задачи не может быть пустым");

            if (name.Length > _maxTaskLength)
                throw new TaskLengthLimitException(name.Length, _maxTaskLength);

            if (_repository.CountActive(user.UserId) >= _maxTaskCount)
                throw new TaskCountLimitException(_maxTaskCount);

            if (_repository.ExistsByName(user.UserId, name))
                throw new DuplicateTaskException(name);

            var item = new ToDoItem(user, name);
            _repository.Add(item);
            return item;
        }

        public void MarkCompleted(Guid id)
        {
            var item = _repository.Get(id);
            if (item == null)
                throw new ArgumentException("Задача не найдена");

            item.MarkAsCompleted();
            _repository.Update(item);
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId) =>
            _repository.GetAllByUserId(userId);

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) =>
            _repository.GetActiveByUserId(userId);

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            string prefix = namePrefix.Trim().ToLower();
            return _repository.Find(user.UserId, t => t.Name.ToLower().StartsWith(prefix));
        }
    }
}


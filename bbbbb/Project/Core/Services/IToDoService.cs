using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Services
{
    public interface IToDoService
    {
       
            ToDoItem Add(ToDoUser user, string name);
            void MarkCompleted(Guid id);
            IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
            IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);

            // поиск по префиксу
            IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix);
        
    }
}

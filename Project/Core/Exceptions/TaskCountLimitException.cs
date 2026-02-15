using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbbbb.Project.Core.Exceptions
{

    public class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int limit)
            : base($"Превышен лимит задач: {limit}") { }
    }
}

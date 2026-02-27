using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbbbb.Project.Core.Exceptions
{
    public class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int actualLength, int maxLength)
            : base($"Длина задачи ({actualLength}) превышает максимально допустимую ({maxLength}).") { }
    }
}

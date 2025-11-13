using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Services
{
    public interface IUserService
    {
        Task<ToDoUser> GetOrCreateUserAsync(long telegramUserId, string? username, CancellationToken cancellationToken);
    }
}

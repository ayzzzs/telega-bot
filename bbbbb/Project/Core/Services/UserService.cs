using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bbbbb.Project.Core.DataAccess;
using bbbbb.Project.Core.Entities;

namespace bbbbb.Project.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var existing = _repository.GetUserByTelegramUserId(telegramUserId);
            if (existing != null)
                return existing;

            var user = new ToDoUser(telegramUserId, telegramUserName);
            _repository.Add(user);
            return user;
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId) =>
            _repository.GetUserByTelegramUserId(telegramUserId);
    }
}

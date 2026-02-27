using System;
using System;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.DataAccess.Models;

namespace bbbbb.Project.Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            if (model == null) return null;

            return new ToDoUser
            {
                UserId = model.UserId,
                TelegramUserId = model.TelegramUserId,
                Username = model.Username
            };
        }

        public static ToDoUserModel MapToModel(ToDoUser entity)
        {
            if (entity == null) return null;

            return new ToDoUserModel
            {
                UserId = entity.UserId,
                TelegramUserId = entity.TelegramUserId,
                Username = entity.Username,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            if (model == null) return null;

            return new ToDoItem
            {
                Id = model.Id,
                Name = model.Name,
                Deadline = model.Deadline,
                State = (ToDoState)model.State,
                User = MapFromModel(model.User),
                List = model.List != null ? MapFromModel(model.List) : null
            };
        }

        public static ToDoItemModel MapToModel(ToDoItem entity)
        {
            if (entity == null) return null;

            return new ToDoItemModel
            {
                Id = entity.Id,
                Name = entity.Name,
                UserId = entity.User.UserId,
                ListId = entity.List?.Id,
                Deadline = entity.Deadline,
                State = (int)entity.State,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static ToDoList MapFromModel(ToDoListModel model)
        {
            if (model == null) return null;

            return new ToDoList
            {
                Id = model.Id,
                Name = model.Name,
                User = MapFromModel(model.User),
                CreatedAt = model.CreatedAt
            };
        }

        public static ToDoListModel MapToModel(ToDoList entity)
        {
            if (entity == null) return null;

            return new ToDoListModel
            {
                Id = entity.Id,
                Name = entity.Name,
                UserId = entity.User.UserId,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
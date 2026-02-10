using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp3.Project.TelegramBot.Dto
{
    public class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }

        public ToDoListCallbackDto(string action, Guid? toDoListId = null) : base(action)
        {
            ToDoListId = toDoListId;
        }

        public static new ToDoListCallbackDto FromString(string input)
        {
            var parts = input.Split('|');
            var action = parts[0];
            Guid? listId = null;

            if (parts.Length > 1 && Guid.TryParse(parts[1], out var parsed))
                listId = parsed;

            return new ToDoListCallbackDto(action, listId);
        }

        public override string ToString() => $"{base.ToString()}|{ToDoListId}";
    }
}

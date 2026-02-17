using System;

namespace bbbbb.Project.TelegramBot.Dto
{
    public class ToDoItemCallbackDto : CallbackDto
    {
        public Guid ToDoItemId { get; set; }

        public ToDoItemCallbackDto(string action, Guid toDoItemId) : base(action)
        {
            ToDoItemId = toDoItemId;
        }

        public static new ToDoItemCallbackDto FromString(string input)
        {
            var parts = input.Split('|');
            var action = parts[0];
            var itemId = Guid.Parse(parts[1]);

            return new ToDoItemCallbackDto(action, itemId);
        }

        public override string ToString() => $"{base.ToString()}|{ToDoItemId}";
    }
}
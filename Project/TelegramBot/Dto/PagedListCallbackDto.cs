using System;


namespace bbbbb.Project.TelegramBot.Dto
{
    public class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }

        public PagedListCallbackDto(string action, Guid? toDoListId, int page) : base(action, toDoListId)
        {
            Page = page;
        }

        public static new PagedListCallbackDto FromString(string input)
        {
            var parts = input.Split('|');
            var action = parts[0];
            Guid? listId = null;

            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) && Guid.TryParse(parts[1], out var parsed))
                listId = parsed;

            int page = 0;
            if (parts.Length > 2 && int.TryParse(parts[2], out var parsedPage))
                page = parsedPage;

            return new PagedListCallbackDto(action, listId, page);
        }

        public override string ToString() => $"{base.ToString()}|{Page}";
    }
}
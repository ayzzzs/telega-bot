using System;

namespace bbbbb.Project.TelegramBot.Dto
{
    public class CallbackDto
    {
        public string Action { get; set; }

        public CallbackDto(string action)
        {
            Action = action;
        }

        public static CallbackDto FromString(string input)
        {
            if (!input.Contains('|'))
                return new CallbackDto(input);

            var parts = input.Split('|');
            return new CallbackDto(parts[0]);
        }

        public override string ToString() => Action;
    }
}
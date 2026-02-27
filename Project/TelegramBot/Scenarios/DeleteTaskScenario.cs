using System;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Services;
using bbbbb.Project.TelegramBot.Dto;
using bbbbb.Project.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConsoleApp3.Project.TelegramBot.Scenarios
{
    public class DeleteTaskScenario : IScenario
    {
        private readonly IToDoService _todoService;

        public DeleteTaskScenario(IToDoService todoService)
        {
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteTask;

        public Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Message message,
            CancellationToken ct)
            => Task.FromResult(ScenarioResult.Transition);

        public async Task<ScenarioResult> HandleCallbackAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            CallbackQuery callbackQuery,
            CancellationToken ct)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var messageId = callbackQuery.Message.MessageId;
            var data = callbackQuery.Data ?? "";

            switch (context.CurrentStep)
            {
                case null:
                    var dto = ToDoItemCallbackDto.FromString(data);
                    var task = await _todoService.Get(dto.ToDoItemId, ct);

                    if (task == null)
                    {
                        await bot.AnswerCallbackQuery(callbackQuery.Id, "Задача не найдена", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["Task"] = task;
                    context.CurrentStep = "Confirm";

                    await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                    await bot.EditMessageText(
                        chatId,
                        messageId,
                        $"Подтверждаете удаление задачи '{task.Name}'?",
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                                InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                            }
                        }),
                        cancellationToken: ct
                    );
                    return ScenarioResult.Transition;

                case "Confirm":
                    await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

                    if (data == "yes")
                    {
                        var taskToDelete = (ToDoItem)context.Data["Task"];
                        await _todoService.DeleteAsync(taskToDelete.Id, ct);
                        await bot.EditMessageText(chatId, messageId, $"✅ Задача '{taskToDelete.Name}' удалена!", cancellationToken: ct);
                    }
                    else
                    {
                        await bot.EditMessageText(chatId, messageId, "Удаление отменено", cancellationToken: ct);
                    }
                    return ScenarioResult.Completed;

                default:
                    return ScenarioResult.Completed;
            }
        }
    }
}

using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using ConsoleApp3.Project.Core.Services;
using bbbbb.Project.TelegramBot.Scenarios;
using ConsoleApp3.Project.TelegramBot.Dto;
using ConsoleApp3.Project.Core.Entities;

namespace ConsoleApp3.Project.TelegramBot
{
    public class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _todoListService;
        private readonly IToDoService _todoService;

        public DeleteListScenario(IUserService userService, IToDoListService todoListService, IToDoService todoService)
        {
            _userService = userService;
            _todoListService = todoListService;
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteList;

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Message message,
            CancellationToken ct)
        {
            // DeleteListScenario работает через CallbackQuery
            return ScenarioResult.Transition;
        }

        public async Task<ScenarioResult> HandleCallbackAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            CallbackQuery callbackQuery,
            CancellationToken ct)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var data = callbackQuery.Data ?? "";

            switch (context.CurrentStep)
            {
                case null:
                    var user = await _userService.GetByTelegramIdAsync(callbackQuery.From.Id, ct);
                    if (user == null)
                    {
                        await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["User"] = user;
                    context.CurrentStep = "Approve";

                    var lists = await _todoListService.GetUserLists(user.UserId, ct);
                    var keyboard = BuildDeleteKeyboard(lists);

                    await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                    await bot.SendMessage(
                        chatId,
                        "Выберите список для удаления:",
                        replyMarkup: keyboard,
                        cancellationToken: ct
                    );
                    return ScenarioResult.Transition;

                case "Approve":
                    var dto = ToDoListCallbackDto.FromString(data);
                    if (!dto.ToDoListId.HasValue)
                        return ScenarioResult.Transition;

                    var selectedList = await _todoListService.Get(dto.ToDoListId.Value, ct);
                    if (selectedList == null)
                    {
                        await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                        await bot.SendMessage(chatId, "Список не найден", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["List"] = selectedList;
                    context.CurrentStep = "Delete";

                    await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                    await bot.SendMessage(
                        chatId,
                        $"Подтверждаете удаление списка '{selectedList.Name}' и всех его задач?",
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

                case "Delete":
                    await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

                    if (data == "yes")
                    {
                        var listToDelete = (ToDoList)context.Data["List"];
                        var savedUser = (ToDoUser)context.Data["User"];

                        // Удаляем все задачи из этого списка
                        var tasks = await _todoService.GetByUserIdAndList(savedUser.UserId, listToDelete.Id, ct);
                        foreach (var task in tasks)
                            await _todoService.DeleteAsync(task.Id, ct);

                        // Удаляем список
                        await _todoListService.Delete(listToDelete.Id, ct);

                        await bot.SendMessage(chatId, $"✅ Список '{listToDelete.Name}' и все его задачи удалены!", cancellationToken: ct);
                    }
                    else
                    {
                        await bot.SendMessage(chatId, "Удаление отменено", cancellationToken: ct);
                    }

                    return ScenarioResult.Completed;

                default:
                    return ScenarioResult.Completed;
            }
        }

        private InlineKeyboardMarkup BuildDeleteKeyboard(IReadOnlyList<ToDoList> lists)
        {
            var rows = new List<InlineKeyboardButton[]>();
            foreach (var list in lists)
            {
                var dto = new ToDoListCallbackDto("deletelist", list.Id);
                rows.Add(new[] { InlineKeyboardButton.WithCallbackData(list.Name, dto.ToString()) });
            }
            return new InlineKeyboardMarkup(rows);
        }
    }
}

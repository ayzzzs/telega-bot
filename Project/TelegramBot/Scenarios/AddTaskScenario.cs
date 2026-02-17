using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Exceptions;
using bbbbb.Project.Core.Services;
using bbbbb.Project.TelegramBot.Dto;
using bbbbb.Project.TelegramBot.Scenarios;
using ConsoleApp3.Project.Core.Entities;
using ConsoleApp3.Project.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConsoleApp3.Project.TelegramBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoListService _todoListService;

        public AddTaskScenario(IUserService userService, IToDoService todoService, IToDoListService todoListService)
        {
            _userService = userService;
            _todoService = todoService;
            _todoListService = todoListService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddTask;

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Message message,
            CancellationToken ct)
        {
            var chatId = message.Chat.Id;
            var text = message.Text ?? "";

            switch (context.CurrentStep)
            {
                case null:
                    var user = await _userService.GetByTelegramIdAsync(message.From!.Id, ct);
                    if (user == null)
                    {
                        await bot.SendMessage(chatId, "Ошибка: пользователь не найден", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["User"] = user;
                    context.CurrentStep = "Name";

                    await bot.SendMessage(
                        chatId,
                        "Введите название задачи:",
                        replyMarkup: GetCancelKeyboard(),
                        cancellationToken: ct
                    );
                    return ScenarioResult.Transition;

                case "Name":
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        await bot.SendMessage(chatId, "Название не может быть пустым. Попробуйте еще раз:", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    context.Data["Name"] = text;
                    context.CurrentStep = "Deadline";

                    await bot.SendMessage(chatId, "Введите дедлайн в формате dd.MM.yyyy:", cancellationToken: ct);
                    return ScenarioResult.Transition;

                case "Deadline":
                    if (!DateTime.TryParseExact(text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var deadline))
                    {
                        await bot.SendMessage(
                            chatId,
                            "Неверный формат даты. Используйте dd.MM.yyyy (например, 25.12.2024):",
                            cancellationToken: ct
                        );
                        return ScenarioResult.Transition;
                    }

                    context.Data["Deadline"] = deadline;
                    context.CurrentStep = "List";

                    var savedUser = (ToDoUser)context.Data["User"];
                    var lists = await _todoListService.GetUserLists(savedUser.UserId, ct);
                    var keyboard = BuildListKeyboard(lists);

                    await bot.SendMessage(chatId, "Выберите список:", replyMarkup: keyboard, cancellationToken: ct);
                    return ScenarioResult.Transition;

                default:
                    await bot.SendMessage(chatId, "Неизвестный шаг сценария", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }

        public async Task<ScenarioResult> HandleCallbackAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            CallbackQuery callbackQuery,
            CancellationToken ct)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var data = callbackQuery.Data ?? "";

            if (context.CurrentStep != "List")
                return ScenarioResult.Completed;

            var dto = ToDoListCallbackDto.FromString(data);
            var savedUser = (ToDoUser)context.Data["User"];
            var savedName = (string)context.Data["Name"];
            var savedDeadline = (DateTime)context.Data["Deadline"];

            ToDoList? selectedList = null;
            if (dto.ToDoListId.HasValue)
                selectedList = await _todoListService.Get(dto.ToDoListId.Value, ct);

            try
            {
                var task = await _todoService.AddTaskAsync(savedUser, savedName, savedDeadline, selectedList, ct);
                var listName = selectedList?.Name ?? "Без списка";

                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                await bot.SendMessage(
                    chatId,
                    $"✅ Задача добавлена!\n\nНазвание: {task.Name}\nДедлайн: {task.Deadline:dd.MM.yyyy}\nСписок: {listName}\nID: {task.Id}",
                    cancellationToken: ct
                );
            }
            catch (DuplicateTaskException ex)
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                await bot.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: ct);
            }
            catch (TaskCountLimitException ex)
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                await bot.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: ct);
            }

            return ScenarioResult.Completed;
        }

        private InlineKeyboardMarkup BuildListKeyboard(IReadOnlyList<ToDoList> lists)
        {
            var rows = new List<InlineKeyboardButton[]>();

            var noListDto = new ToDoListCallbackDto("show", null);
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData("📌Без списка", noListDto.ToString()) });

            rows.AddRange(lists.Select(list =>
            {
                var dto = new ToDoListCallbackDto("show", list.Id);
                return new[] { InlineKeyboardButton.WithCallbackData(list.Name, dto.ToString()) };
            }));

            return new InlineKeyboardMarkup(rows);
        }

        private ReplyKeyboardMarkup GetCancelKeyboard() =>
            new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { new("/cancel") } }) { ResizeKeyboard = true };
    }
}
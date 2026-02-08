using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Exceptions;
using bbbbb.Project.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace bbbbb.Project.TelegramBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;

        public AddTaskScenario(IUserService userService, IToDoService todoService)
        {
            _userService = userService;
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

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
                    // Шаг 1: Запрос названия задачи
                    var user = await _userService.GetByTelegramIdAsync(message.From!.Id, ct);
                    if (user == null)
                    {
                        await bot.SendMessage(chatId, "Ошибка: пользователь не найден", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["User"] = user;
                    context.CurrentStep = "Name";

                    var cancelKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { new KeyboardButton("/cancel") }
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await bot.SendMessage(
                        chatId,
                        "Введите название задачи:",
                        replyMarkup: cancelKeyboard,
                        cancellationToken: ct
                    );

                    return ScenarioResult.Transition;

                case "Name":
                    // Шаг 2: Сохранение названия и запрос дедлайна
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        await bot.SendMessage(chatId, "Название задачи не может быть пустым. Попробуйте еще раз:", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    context.Data["Name"] = text;
                    context.CurrentStep = "Deadline";

                    await bot.SendMessage(
                        chatId,
                        "Введите дедлайн в формате dd.MM.yyyy (например, 25.12.2024):",
                        cancellationToken: ct
                    );

                    return ScenarioResult.Transition;

                case "Deadline":
                    // Шаг 3: Парсинг дедлайна и создание задачи
                    if (!DateTime.TryParseExact(
                        text,
                        "dd.MM.yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var deadline))
                    {
                        await bot.SendMessage(
                            chatId,
                            "Неверный формат даты. Используйте формат dd.MM.yyyy (например, 25.12.2024):",
                            cancellationToken: ct
                        );
                        return ScenarioResult.Transition;
                    }

                    var savedUser = (ToDoUser)context.Data["User"];
                    var savedName = (string)context.Data["Name"];

                    try
                    {
                        var task = await _todoService.AddTaskAsync(savedUser, savedName, deadline, ct);

                        await bot.SendMessage(
                            chatId,
                            $"✅ Задача добавлена!\n\nНазвание: {task.Name}\nДедлайн: {task.Deadline:dd.MM.yyyy}\nID: {task.Id}",
                            cancellationToken: ct
                        );

                        return ScenarioResult.Completed;
                    }
                    catch (DuplicateTaskException ex)
                    {
                        await bot.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    catch (TaskCountLimitException ex)
                    {
                        await bot.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                default:
                    await bot.SendMessage(chatId, "Неизвестный шаг сценария", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
    }
}
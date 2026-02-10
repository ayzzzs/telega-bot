
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using ConsoleApp3.Project.Core.Services;
using bbbbb.Project.TelegramBot.Scenarios;

namespace ConsoleApp3.Project.TelegramBot
{
    public class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _todoListService;

        public AddListScenario(IUserService userService, IToDoListService todoListService)
        {
            _userService = userService;
            _todoListService = todoListService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddList;

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
                        "Введите название списка:",
                        replyMarkup: new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { new("/cancel") } }) { ResizeKeyboard = true },
                        cancellationToken: ct
                    );
                    return ScenarioResult.Transition;

                case "Name":
                    var savedUser = (ToDoUser)context.Data["User"];
                    try
                    {
                        var list = await _todoListService.Add(savedUser, text, ct);
                        await bot.SendMessage(
                            chatId,
                            $"✅ Список '{list.Name}' создан!",
                            cancellationToken: ct
                        );
                    }
                    catch (ArgumentException ex)
                    {
                        await bot.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: ct);
                    }
                    return ScenarioResult.Completed;

                default:
                    return ScenarioResult.Completed;
            }
        }

        public Task<ScenarioResult> HandleCallbackAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            CallbackQuery callbackQuery,
            CancellationToken ct)
            => Task.FromResult(ScenarioResult.Completed);
    }
}

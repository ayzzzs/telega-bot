using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Repositories;
using bbbbb.Project.Core.Services;
using bbbbb.Project.Infrastructure.DataAccess;
using bbbbb.Project.TelegramBot.Scenarios;
using bbbbb.Project.TelegramBot.Scenarios.bbbbb.Project.TelegramBot.Scenarios;
using ConsoleApp3.Project.Core.Repositories;
using ConsoleApp3.Project.Core.Services;
using ConsoleApp3.Project.Infrastructure.DataAccess;
using ConsoleApp3.Project.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.Write("Введите токен бота: ");
        var token = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("Токен не может быть пустым!");
            Console.ReadKey();
            return;
        }

        IUserRepository userRepo = new FileUserRepository("data/users");
        IToDoRepository todoRepo = new FileToDoRepository("data/todos");
        IToDoListRepository todoListRepo = new FileToDoListRepository("data/todolists");

        IUserService userService = new UserService(userRepo);
        IToDoService todoService = new ToDoService(todoRepo);
        IToDoReportService reportService = new ToDoReportService(todoRepo);
        IToDoListService todoListService = new ToDoListService(todoListRepo);

        IScenarioContextRepository contextRepo = new InMemoryScenarioContextRepository();
        var scenarios = new List<IScenario>
        {
            new AddTaskScenario(userService, todoService, todoListService),
            new AddListScenario(userService, todoListService),
            new DeleteListScenario(userService, todoListService, todoService),
            new DeleteTaskScenario(todoService)
        };

        var handler = new UpdateHandler(userService, todoService, reportService, todoListService, scenarios, contextRepo);

        var botClient = new TelegramBotClient(token);
        var cts = new CancellationTokenSource();

        Console.WriteLine("Подключение к Telegram...");

        try
        {
            var me = await botClient.GetMe(cts.Token);

            // Устанавливаем команды бота
            await botClient.SetMyCommands(
                new[]
                {
                    new BotCommand { Command = "start", Description = "Начать работу" },
                    new BotCommand { Command = "help", Description = "Помощь" },
                    new BotCommand { Command = "addtask", Description = "Добавить задачу" },
                    new BotCommand { Command = "show", Description = "Показать задачи" },
                    new BotCommand { Command = "report", Description = "Статистика" },
                    new BotCommand { Command = "cancel", Description = "Отменить операцию" }
                },
                cancellationToken: cts.Token
            );

            Console.WriteLine($"\n✓ Бот @{me.Username} запущен!");
            Console.WriteLine("Нажмите Ctrl+C для остановки");
            Console.WriteLine(new string('=', 50));

            botClient.StartReceiving(
                handler,
                new ReceiverOptions
                {
                    AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
                },
                cts.Token
            );

            var tcs = new TaskCompletionSource<bool>();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                tcs.TrySetResult(true);
            };

            await tcs.Task;
            cts.Cancel();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка: {ex.Message}");
            Console.ReadKey();
        }
    }
}
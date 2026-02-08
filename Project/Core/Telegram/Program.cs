using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using bbbbb.Project.Core.Repositories;
using bbbbb.Project.Core.Services;
using bbbbb.Project.Infrastructure.DataAccess;
using bbbbb.Project.TelegramBot.Scenarios;

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

        IUserService userService = new UserService(userRepo);
        IToDoService todoService = new ToDoService(todoRepo);
        IToDoReportService reportService = new ToDoReportService(todoRepo);

        // Инициализация сценариев
        IScenarioContextRepository contextRepo = new InMemoryScenarioContextRepository();
        var scenarios = new List<IScenario>
        {
            new AddTaskScenario(userService, todoService)
        };

        var handler = new UpdateHandler(userService, todoService, reportService, scenarios, contextRepo);

        var botClient = new TelegramBotClient(token);
        var cts = new CancellationTokenSource();

        Console.WriteLine("Подключение к Telegram...");

        try
        {
            var me = await botClient.GetMe(cts.Token);
            Console.WriteLine($"\n✓ Бот @{me.Username} успешно запущен!");
            Console.WriteLine($"ID бота: {me.Id}");
            Console.WriteLine($"Имя: {me.FirstName}");
            Console.WriteLine("\nБот готов к приему сообщений...");
            Console.WriteLine("Нажмите Ctrl+C для остановки");
            Console.WriteLine(new string('=', 50));

            botClient.StartReceiving(
                handler,
                new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message } },
                cts.Token
            );

            var tcs = new TaskCompletionSource<bool>();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                tcs.TrySetResult(true);
            };

            await tcs.Task;

            Console.WriteLine("\nОстановка бота...");
            cts.Cancel();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка запуска бота: {ex.Message}");
            Console.ReadKey();
        }
    }
}
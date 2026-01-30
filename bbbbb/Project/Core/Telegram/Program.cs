using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using bbbbb.Project.Core.Repositories;
using bbbbb.Project.Core.Services;

class Program
{
    static async Task Main()
    {
        Console.Write("Введите токен: ");
        var token = Console.ReadLine();

        var botClient = new TelegramBotClient(token);

        IUserRepository userRepo = new FileUserRepository("data/users");
        IToDoRepository todoRepo = new FileToDoRepository("data/todos");

        IUserService userService = new UserService(userRepo);
        IToDoService todoService = new ToDoService(todoRepo);
        IToDoReportService reportService = new ToDoReportService(todoRepo);

        var handler = new UpdateHandler(userService, todoService, reportService);

        var cts = new CancellationTokenSource();

        botClient.StartReceiving(
            handler.HandleUpdateAsync,
            handler.HandlePollingErrorAsync,
            new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message } },
            cts.Token
        );

        var me = await botClient.GetMe(cts.Token);
        Console.WriteLine($"{me.Username} запущен!");
        Console.WriteLine("Нажмите A для выхода");

        while (true)
        {
            if (Console.ReadKey(true).Key == ConsoleKey.A)
            {
                cts.Cancel();
                break;
            }
        }
    }
}
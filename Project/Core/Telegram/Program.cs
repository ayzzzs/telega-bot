using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using bbbbb.Project.Core.Repositories;
using bbbbb.Project.Core.Services;
using bbbbb.Project.Infrastructure.DataAccess;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.Write("Введите токен: ");
        var token = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("Токен не может быть пустым!");
            Console.ReadKey();
            return;
        }

        var botClient = new TelegramBotClient(token);
        IUserRepository userRepo = new FileUserRepository("data/users");
        IToDoRepository todoRepo = new FileToDoRepository("data/todos");

        IUserService userService = new UserService(userRepo);
        IToDoService todoService = new ToDoService(todoRepo);
        IToDoReportService reportService = new ToDoReportService(todoRepo);

        var handler = new UpdateHandler(userService, todoService, reportService);

        var cts = new CancellationTokenSource();

        Console.WriteLine("Подключение к Telegram...");

        try
        {
            var me = await botClient.GetMe(cts.Token);
            Console.WriteLine($"✓ Бот @{me.Username} успешно запущен!");
            Console.WriteLine($"ID бота: {me.Id}");
            Console.WriteLine($"Имя: {me.FirstName}");
            Console.WriteLine();
            Console.WriteLine("Бот готов к приему сообщений...");
            Console.WriteLine("Нажмите Ctrl+C для остановки");
            Console.WriteLine(new string('=', 50));

            botClient.StartReceiving(
                handler,
                new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message } },
                cts.Token
            );

            // Ожидание нажатия Ctrl+C
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
            Console.WriteLine("Проверьте правильность токена!");
            Console.ReadKey();
        }
    }
}
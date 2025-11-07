
using bbbbb.Project.Core.TelegramBot;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Infrastructure.DataAccess;
using bbbbb.Project.Core.Services;


class Program
{

    static void Main()
    {
        var userRepository = new InMemoryUserRepository();
        var todoRepository = new InMemoryToDoRepository();

        // создаём сервисы
        var userService = new UserService(userRepository);
        var todoService = new ToDoService(todoRepository);
        var reportService = new ToDoReportService(todoRepository);

        // создаём обработчик команд
        var handler = new UpdateHandler(userService, todoService, reportService);

        // запускаем консольный бот
        var botClient = new ConsoleBotClient();
        botClient.StartReceiving(handler);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using bbbbb;
using Otus.ToDoList.ConsoleBot;

class Program
{

    static void Main()
    {
        IUserService userService = new UserService();
        IToDoService todoService = new ToDoService();
        var handler = new UpdateHandler(userService, todoService);
        ITelegramBotClient botClient = new ConsoleBotClient();

        botClient.StartReceiving(handler); // тут программа “живёт” в консоли
    }
}

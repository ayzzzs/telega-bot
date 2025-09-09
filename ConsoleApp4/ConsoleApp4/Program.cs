using System;
using System.Collections.Generic;
using System.Linq;

class TelegramBotMenu
{
    static string userName = "";
    static bool isStarted = false;
    static readonly string version = "1.2";
    static readonly string creationDate = "2024-01-15";
    static List<string> tasks = new List<string>();
    static List<string> commandHistory = new List<string>();
    static DateTime startTime;

    static void Main(string[] args)
    {
        startTime = DateTime.Now;
        ShowWelcomeMessage();

        while (true)
        {
            Console.Write("Введите команду: ");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                continue;

            // Добавляем команду в историю
            commandHistory.Add(input);

            string[] parts = input.Split(' ', 2);
            string command = parts[0].ToLower();
            string argument = parts.Length > 1 ? parts[1] : "";

            switch (command)
            {
                case "/start":
                    HandleStartCommand();
                    break;
                case "/help":
                    HandleHelpCommand();
                    break;
                case "/info":
                    HandleInfoCommand();
                    break;
                case "/echo":
                    HandleEchoCommand(argument);
                    break;
                case "/addtask":
                    HandleAddTaskCommand();
                    break;
                case "/showtasks":
                    HandleShowTasksCommand();
                    break;
                case "/removetask":
                    HandleRemoveTaskCommand();
                    break;
                case "/history":
                    HandleHistoryCommand();
                    break;
                case "/stats":
                    HandleStatsCommand();
                    break;
                case "/exit":
                    HandleExitCommand();
                    return;
                case "/clear":
                    Console.Clear();
                    ShowWelcomeMessage();
                    break;
                default:
                    Console.WriteLine("Неизвестная команда. Введите /help для списка доступных команд.");
                    break;
            }

            Console.WriteLine(); // Пустая строка для читаемости
        }
    }

    static void ShowWelcomeMessage()
    {
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║         ТЕЛЕГРАММ БОТ                ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine("Доступные команды:");
        Console.WriteLine("• /start - начать работу с ботом");
        Console.WriteLine("• /help - показать справку");
        Console.WriteLine("• /info - информация о программе");
        Console.WriteLine("• /echo [текст] - повторить текст");
        Console.WriteLine("• /addtask - добавить задачу");
        Console.WriteLine("• /showtasks - показать все задачи");
        Console.WriteLine("• /removetask - удалить задачу");
        Console.WriteLine("• /history - история команд");
        Console.WriteLine("• /stats - статистика работы");
        Console.WriteLine("• /clear - очистить экран");
        Console.WriteLine("• /exit - выйти из программы");
        Console.WriteLine();
    }

    static void HandleStartCommand()
    {
        if (isStarted)
        {
            Console.WriteLine("Бот уже запущен!");
            return;
        }

        Console.Write("Пожалуйста, введите ваше имя: ");
        userName = Console.ReadLine();

        if (!string.IsNullOrEmpty(userName))
        {
            isStarted = true;
            Console.WriteLine($" Привет, {userName}! Теперь доступны все команды бота.");
            Console.WriteLine($" Всего задач в списке: {tasks.Count}");
        }
        else
        {
            Console.WriteLine(" Имя не может быть пустым. Попробуйте снова.");
        }
    }

    static void HandleHelpCommand()
    {
        Console.WriteLine(" СПРАВКА ПО КОМАНДАМ:");
        Console.WriteLine("────────────────────────");
        Console.WriteLine("/start - начать работу с ботом (требуется ввести имя)");
        Console.WriteLine("/help - показать эту справку");
        Console.WriteLine("/info - информация о версии программы");
        Console.WriteLine("/echo [текст] - повторить введенный текст");
        Console.WriteLine("/addtask - добавить новую задачу в список");
        Console.WriteLine("/showtasks - показать все текущие задачи");
        Console.WriteLine("/removetask - удалить задачу по номеру");
        Console.WriteLine("/history - показать историю введенных команд");
        Console.WriteLine("/stats - статистика работы бота");
        Console.WriteLine("/clear - очистить экран консоли");
        Console.WriteLine("/exit - завершить работу программы");

        if (isStarted)
        {
            Console.WriteLine($"────────────────────────");
            Console.WriteLine($" Текущий пользователь: {userName}");
            Console.WriteLine($" Всего задач: {tasks.Count}");
        }
    }

    static void HandleInfoCommand()
    {
        Console.WriteLine("  ИНФОРМАЦИЯ О ПРОГРАММЕ:");
        Console.WriteLine($"Версия: {version}");
        Console.WriteLine($"Дата создания: {creationDate}");
        Console.WriteLine($"Время работы: {(DateTime.Now - startTime):hh\\:mm\\:ss}");
        Console.WriteLine("Простой Телеграмм бот с управлением задачами");
    }

    static void HandleEchoCommand(string argument)
    {
        if (!isStarted)
        {
            Console.WriteLine(" Сначала запустите бот командой /start!");
            return;
        }

        if (string.IsNullOrEmpty(argument))
        {
            Console.WriteLine($"{userName}, пожалуйста, введите текст после команды /echo");
        }
        else
        {
            Console.WriteLine($"{userName}, вы сказали: \"{argument}\"");
        }
    }

    static void HandleAddTaskCommand()
    {
        if (!isStarted)
        {
            Console.WriteLine("❌ Сначала запустите бот командой /start!");
            return;
        }

        Console.Write(" Пожалуйста, введите описание задачи: ");
        string taskDescription = Console.ReadLine();

        if (!string.IsNullOrEmpty(taskDescription))
        {
            tasks.Add(taskDescription);
            Console.WriteLine($"Задача \"{taskDescription}\" добавлена.");
            Console.WriteLine($" Всего задач: {tasks.Count}");
        }
        else
        {
            Console.WriteLine(" Описание задачи не может быть пустым.");
        }
    }

    static void HandleShowTasksCommand()
    {
        if (!isStarted)
        {
            Console.WriteLine(" Сначала запустите бот командой /start!");
            return;
        }

        if (tasks.Count == 0)
        {
            Console.WriteLine(" Список задач пуст.");
            return;
        }

        Console.WriteLine(" ВАШИ ЗАДАЧИ:");
        Console.WriteLine("────────────────────────");
        for (int i = 0; i < tasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {tasks[i]}");
        }
        Console.WriteLine($"────────────────────────");
        Console.WriteLine($"Всего задач: {tasks.Count}");
    }

    static void HandleRemoveTaskCommand()
    {
        if (!isStarted)
        {
            Console.WriteLine(" Сначала запустите бот командой /start!");
            return;
        }

        if (tasks.Count == 0)
        {
            Console.WriteLine(" Список задач пуст. Нечего удалять.");
            return;
        }

        // Показываем список задач
        HandleShowTasksCommand();
        Console.WriteLine();

        Console.Write(" Введите номер задачи для удаления: ");
        string input = Console.ReadLine();

        if (int.TryParse(input, out int taskNumber))
        {
            if (taskNumber >= 1 && taskNumber <= tasks.Count)
            {
                string removedTask = tasks[taskNumber - 1];
                tasks.RemoveAt(taskNumber - 1);
                Console.WriteLine($"Задача \"{removedTask}\" удалена.");
                Console.WriteLine($" Осталось задач: {tasks.Count}");
            }
            else
            {
                Console.WriteLine($" Неверный номер задачи. Введите число от 1 до {tasks.Count}.");
            }
        }
        else
        {
            Console.WriteLine(" Пожалуйста, введите корректный номер задачи.");
        }
    }

    static void HandleHistoryCommand()
    {
        if (commandHistory.Count == 0)
        {
            Console.WriteLine(" История команд пуста.");
            return;
        }

        Console.WriteLine(" ИСТОРИЯ КОМАНД:");
        Console.WriteLine("────────────────────────");
        for (int i = 0; i < commandHistory.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {commandHistory[i]}");
        }
    }

    static void HandleStatsCommand()
    {
        Console.WriteLine(" СТАТИСТИКА БОТА:");
        Console.WriteLine("────────────────────────");
        Console.WriteLine($"Время работы: {(DateTime.Now - startTime):hh\\:mm\\:ss}");
        Console.WriteLine($"Всего команд выполнено: {commandHistory.Count}");
        Console.WriteLine($"Количество задач: {tasks.Count}");
        Console.WriteLine($"Пользователь: {(isStarted ? userName : "Не авторизован")}");
    }

    static void HandleExitCommand()
    {
        Console.WriteLine("────────────────────────");
        if (!string.IsNullOrEmpty(userName))
        {
            Console.WriteLine($" До свидания, {userName}! Ждем вас снова!");
        }
        else
        {
            Console.WriteLine(" До свидания! Ждем вас снова!");
        }

        Console.WriteLine($" Статистика сессии:");
        Console.WriteLine($"- Время работы: {(DateTime.Now - startTime):hh\\:mm\\:ss}");
        Console.WriteLine($"- Выполнено команд: {commandHistory.Count}");
        Console.WriteLine($"- Создано задач: {tasks.Count}");

        Console.WriteLine(" Программа завершена.");
    }
}
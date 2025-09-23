using System;
using System.Collections.Generic;
using System.Linq;

// Пользовательские исключения
public class TaskCountLimitException : Exception
{
    public int TaskCountLimit { get; }

    public TaskCountLimitException(int taskCountLimit)
        : base($"Превышено максимальное количество задач равное {taskCountLimit}")
    {
        TaskCountLimit = taskCountLimit;
    }
}

public class TaskLengthLimitException : Exception
{
    public int TaskLength { get; }
    public int TaskLengthLimit { get; }

    public TaskLengthLimitException(int taskLength, int taskLengthLimit)
        : base($"Длина задачи '{taskLength}' превышает максимально допустимое значение {taskLengthLimit}")
    {
        TaskLength = taskLength;
        TaskLengthLimit = taskLengthLimit;
    }
}

public class DuplicateTaskException : Exception
{
    public string Task { get; }

    public DuplicateTaskException(string task)
        : base($"Задача '{task}' уже существует")
    {
        Task = task;
    }
}

class TelegramBotMenu
{
    static string userName = "";
    static bool isStarted = false;
    static readonly string version = "1.3";
    static readonly string creationDate = "2024-01-15";
    static List<string> tasks = new List<string>();
    static List<string> commandHistory = new List<string>();
    static DateTime startTime;

    // Ограничения
    static int maxTaskCount = 10;
    static int maxTaskLength = 50;

    static void Main(string[] args)
    {
        try
        {
            // Запрос ограничений при старте
            Console.Write("Введите максимально допустимое количество задач (1-100): ");
            string maxCountInput = Console.ReadLine();
            maxTaskCount = ParseAndValidateInt(maxCountInput, 1, 100);

            Console.Write("Введите максимально допустимую длину задачи (1-100): ");
            string maxLengthInput = Console.ReadLine();
            maxTaskLength = ParseAndValidateInt(maxLengthInput, 1, 100);

            startTime = DateTime.Now;
            ShowWelcomeMessage();

            MainLoop();
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"❌ Ошибка валидации: {ex.Message}");
            Console.WriteLine("Программа будет использовать значения по умолчанию: макс. задач = 10, макс. длина = 50");
            maxTaskCount = 10;
            maxTaskLength = 50;
            startTime = DateTime.Now;
            ShowWelcomeMessage();
            MainLoop();
        }
        catch (TaskCountLimitException ex)
        {
            Console.WriteLine($"❌ {ex.Message}");
        }
        catch (TaskLengthLimitException ex)
        {
            Console.WriteLine($"❌ {ex.Message}");
        }
        catch (DuplicateTaskException ex)
        {
            Console.WriteLine($"❌ {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Произошла непредвиденная ошибка:");
            Console.WriteLine($"Тип: {ex.GetType()}");
            Console.WriteLine($"Сообщение: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            Console.WriteLine($"InnerException: {ex.InnerException?.Message ?? "Нет"}");
        }
    }

    static void MainLoop()
    {
        while (true)
        {
            try
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

                ProcessCommand(command, argument);
                Console.WriteLine();
            }
            catch (TaskCountLimitException ex)
            {
                Console.WriteLine($"❌ {ex.Message}");
            }
            catch (TaskLengthLimitException ex)
            {
                Console.WriteLine($"❌ {ex.Message}");
            }
            catch (DuplicateTaskException ex)
            {
                Console.WriteLine($"❌ {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"❌ Ошибка ввода: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Ошибка при выполнении команды: {ex.Message}");
            }
        }
    }

    static void ProcessCommand(string command, string argument)
    {
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
            case "/limits":
                HandleLimitsCommand();
                break;
            case "/exit":
                HandleExitCommand();
                Environment.Exit(0);
                break;
            case "/clear":
                Console.Clear();
                ShowWelcomeMessage();
                break;
            default:
                Console.WriteLine("Неизвестная команда. Введите /help для списка доступных команд.");
                break;
        }
    }

    // Валидационные методы
    static int ParseAndValidateInt(string? str, int min, int max)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new ArgumentException("Ввод не может быть пустым");

        if (!int.TryParse(str, out int result))
            throw new ArgumentException($"Некорректное число: '{str}'");

        if (result < min || result > max)
            throw new ArgumentException($"Число должно быть в диапазоне от {min} до {max}");

        return result;
    }

    static void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new ArgumentException("Строка не может быть пустой или состоять только из пробелов");

        if (str.Trim().Length == 0)
            throw new ArgumentException("Строка должна содержать не пробельные символы");
    }

    // Обработчики команд
    static void ShowWelcomeMessage()
    {
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║         ТЕЛЕГРАММ БОТ v1.3          ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine($"Ограничения: макс. задач = {maxTaskCount}, макс. длина = {maxTaskLength}");
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
        Console.WriteLine("• /limits - показать текущие ограничения");
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

        ValidateString(userName);

        isStarted = true;
        Console.WriteLine($"✅ Привет, {userName}! Теперь доступны все команды бота.");
        Console.WriteLine($"📝 Всего задач в списке: {tasks.Count}/{maxTaskCount}");
    }

    static void HandleHelpCommand()
    {
        Console.WriteLine("📖 СПРАВКА ПО КОМАНДАМ:");
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
        Console.WriteLine("/limits - показать текущие ограничения");
        Console.WriteLine("/clear - очистить экран консоли");
        Console.WriteLine("/exit - завершить работу программы");

        if (isStarted)
        {
            Console.WriteLine($"────────────────────────");
            Console.WriteLine($"👤 Текущий пользователь: {userName}");
            Console.WriteLine($"📊 Всего задач: {tasks.Count}/{maxTaskCount}");
        }
    }

    static void HandleInfoCommand()
    {
        Console.WriteLine("ℹ️  ИНФОРМАЦИЯ О ПРОГРАММЕ:");
        Console.WriteLine($"Версия: {version}");
        Console.WriteLine($"Дата создания: {creationDate}");
        Console.WriteLine($"Время работы: {(DateTime.Now - startTime):hh\\:mm\\:ss}");
        Console.WriteLine("Простой Телеграмм бот с управлением задачами и валидацией");
    }

    static void HandleEchoCommand(string argument)
    {
        if (!isStarted)
            throw new InvalidOperationException("Сначала запустите бот командой /start!");

        ValidateString(argument);
        Console.WriteLine($"{userName}, вы сказали: \"{argument}\"");
    }

    static void HandleAddTaskCommand()
    {
        if (!isStarted)
            throw new InvalidOperationException("Сначала запустите бот командой /start!");

        // Проверка ограничения количества задач
        if (tasks.Count >= maxTaskCount)
            throw new TaskCountLimitException(maxTaskCount);

        Console.Write("📝 Пожалуйста, введите описание задачи: ");
        string taskDescription = Console.ReadLine() ?? "";

        ValidateString(taskDescription);

        // Проверка ограничения длины задачи
        if (taskDescription.Length > maxTaskLength)
            throw new TaskLengthLimitException(taskDescription.Length, maxTaskLength);

        // Проверка на дубликаты (без учета регистра и пробелов)
        string normalizedTask = taskDescription.Trim().ToLower();
        if (tasks.Any(t => t.Trim().ToLower() == normalizedTask))
            throw new DuplicateTaskException(taskDescription);

        tasks.Add(taskDescription);
        Console.WriteLine($"✅ Задача \"{taskDescription}\" добавлена.");
        Console.WriteLine($"📊 Всего задач: {tasks.Count}/{maxTaskCount}");
    }

    static void HandleShowTasksCommand()
    {
        if (!isStarted)
            throw new InvalidOperationException("Сначала запустите бот командой /start!");

        if (tasks.Count == 0)
        {
            Console.WriteLine("📭 Список задач пуст.");
            return;
        }

        Console.WriteLine("📋 ВАШИ ЗАДАЧИ:");
        Console.WriteLine("────────────────────────");
        for (int i = 0; i < tasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {tasks[i]}");
        }
        Console.WriteLine($"────────────────────────");
        Console.WriteLine($"Всего задач: {tasks.Count}/{maxTaskCount}");
    }

    static void HandleRemoveTaskCommand()
    {
        if (!isStarted)
            throw new InvalidOperationException("Сначала запустите бот командой /start!");

        if (tasks.Count == 0)
            throw new InvalidOperationException("Список задач пуст. Нечего удалять.");

        // Показываем список задач
        HandleShowTasksCommand();
        Console.WriteLine();

        Console.Write("❌ Введите номер задачи для удаления: ");
        string input = Console.ReadLine() ?? "";

        int taskNumber = ParseAndValidateInt(input, 1, tasks.Count);

        string removedTask = tasks[taskNumber - 1];
        tasks.RemoveAt(taskNumber - 1);
        Console.WriteLine($"✅ Задача \"{removedTask}\" удалена.");
        Console.WriteLine($"📊 Осталось задач: {tasks.Count}/{maxTaskCount}");
    }

    static void HandleHistoryCommand()
    {
        if (commandHistory.Count == 0)
        {
            Console.WriteLine("📭 История команд пуста.");
            return;
        }

        Console.WriteLine("📜 ИСТОРИЯ КОМАНД:");
        Console.WriteLine("────────────────────────");
        for (int i = 0; i < commandHistory.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {commandHistory[i]}");
        }
    }

    static void HandleStatsCommand()
    {
        Console.WriteLine("📊 СТАТИСТИКА БОТА:");
        Console.WriteLine("────────────────────────");
        Console.WriteLine($"Время работы: {(DateTime.Now - startTime):hh\\:mm\\:ss}");
        Console.WriteLine($"Всего команд выполнено: {commandHistory.Count}");
        Console.WriteLine($"Количество задач: {tasks.Count}/{maxTaskCount}");
        Console.WriteLine($"Пользователь: {(isStarted ? userName : "Не авторизован")}");
    }

    static void HandleLimitsCommand()
    {
        Console.WriteLine("⚡ ТЕКУЩИЕ ОГРАНИЧЕНИЯ:");
        Console.WriteLine("────────────────────────");
        Console.WriteLine($"Максимальное количество задач: {maxTaskCount}");
        Console.WriteLine($"Максимальная длина задачи: {maxTaskLength}");
        Console.WriteLine($"Текущее количество задач: {tasks.Count}");
    }

    static void HandleExitCommand()
    {
        Console.WriteLine("────────────────────────");
        if (!string.IsNullOrEmpty(userName))
        {
            Console.WriteLine($"👋 До свидания, {userName}! Ждем вас снова!");
        }
        else
        {
            Console.WriteLine("👋 До свидания! Ждем вас снова!");
        }

        Console.WriteLine($"📊 Статистика сессии:");
        Console.WriteLine($"- Время работы: {(DateTime.Now - startTime):hh\\:mm\\:ss}");
        Console.WriteLine($"- Выполнено команд: {commandHistory.Count}");
        Console.WriteLine($"- Создано задач: {tasks.Count}");

        Console.WriteLine("🚪 Программа завершена.");
    }
}
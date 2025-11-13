using bbbbb.Project.Core.Services;
using bbbbb.Project.Core.Types;
using bbbbb.Project.Core.DataAccess;
using System.Threading;
using System.Threading.Tasks;

public delegate void MessageEventHandler(string message);

public class UpdateHandler : IUpdateHandler
{
    public event MessageEventHandler? OnHandleUpdateStarted;
    public event MessageEventHandler? OnHandleUpdateCompleted;

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message?.Text ?? string.Empty;
        OnHandleUpdateStarted?.Invoke(message);

        try
        {
            if (message.StartsWith("/start"))
                Console.WriteLine("Бот запущен!");
            else
                Console.WriteLine($"Бот:Получил сообщение: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке: {ex.Message}");
        }
        finally
        {
            OnHandleUpdateCompleted?.Invoke(message);
        }

        await Task.CompletedTask;
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        await Task.CompletedTask;
    }
}





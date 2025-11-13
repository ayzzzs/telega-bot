using bbbbb.Project.Core.Types;

namespace bbbbb.Project.Core.DataAccess;
/// <summary>
///  Интерфейс обработчика обновлений для клиента, работающего с ботом
/// </summary>
public interface IUpdateHandler
{
    Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct);
    Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct);
}

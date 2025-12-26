using bbbbb.Project.Core.Types;

namespace bbbbb.Project.Core.DataAccess;
/// <summary>
/// Интерфейс клиента для будущего телеграм-бота
/// </summary>
public interface ITelegramBotClient
{
    Task StartReceiving(IUpdateHandler handler, CancellationToken cancellationToken);
    Task SendMessageAsync(Chat chat, string text, CancellationToken cancellationToken);
}

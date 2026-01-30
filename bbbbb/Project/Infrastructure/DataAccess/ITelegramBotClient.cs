using bbbbb.Project.Core.Types;

namespace bbbbb.Project.Infrastructure.DataAccess;
/// <summary>
/// Интерфейс клиента для будущего телеграм-бота
/// </summary>
public interface ITelegramBotClient
{
    Task StartReceiving(IUpdateHandler handler, CancellationToken cancellationToken);
    Task SendMessageAsync(Chat chat, string text, CancellationToken cancellationToken);
    Task SendMessageAsync(long id, string v, CancellationToken cancellationToken);
}



using Telegram.Bot.Types;

namespace bbbbb.Project.Infrastructure.DataAccess;

public interface ITelegramBotClient
{
    Task StartReceiving(UpdateHandler _handler, CancellationToken cancellationToken);
    Task SendMessageAsync(Chat chat, string text, CancellationToken cancellationToken);
    Task SendMessageAsync(long id, string v, CancellationToken cancellationToken);
}

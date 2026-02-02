//using bbbbb.Project.Core.Types;
//using bbbbb.Project.Infrastructure.DataAccess;

//namespace bbbbb.Project.Core.TelegramBot;
///// <summary>
///// Консольный клиент для бота
///// </summary>

//public class ConsoleBotClient : ITelegramBotClient
//{
//    private readonly Chat _chat;
//    private readonly User _user;

//    public ConsoleBotClient()
//    {
//        _chat = new Chat { Id = Random.Shared.Next() };
//        _user = new User { Id = Random.Shared.Next(), Username = $"ConsoleUser_{Guid.NewGuid()}" };
//    }

//    public async Task SendMessageAsync(Chat chat, string text, CancellationToken cancellationToken)
//    {
//        ArgumentNullException.ThrowIfNull(chat);
//        ArgumentNullException.ThrowIfNull(text);

//        if (_chat.Id != chat.Id)
//            throw new ArgumentException($"Invalid chat.Id. Support {_chat.Id}, but was {chat.Id}");

//        await Task.Run(() =>
//        {
//            WriteLineColor($"Бот: {text}", ConsoleColor.Blue);
//        }, cancellationToken);
//    }

//    public async Task StartReceiving(IUpdateHandler handler, CancellationToken cancellationToken)
//    {
//        ArgumentNullException.ThrowIfNull(handler);

//        Console.CancelKeyPress += (sender, e) =>
//        {
//            e.Cancel = true;
//            cancellationToken.ThrowIfCancellationRequested();
//        };

//        WriteLineColor("Бот запущен. Введите сообщение:", ConsoleColor.Magenta);
//        var counter = 0;

//        while (!cancellationToken.IsCancellationRequested)
//        {
//            var input = Console.ReadLine();
//            if (input is null)
//                break;

//            var update = new Update
//            {
//                Message = new Message
//                {
//                    Id = Interlocked.Increment(ref counter),
//                    Text = input,
//                    Chat = _chat,
//                    From = _user
//                }
//            };

//            try
//            {
//                await handler.HandleUpdateAsync(this, update, cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                await handler.HandleErrorAsync(this, ex, cancellationToken);
//            }
//        }

//        WriteLineColor("Бот остановлен", ConsoleColor.Magenta);
//    }

//    private static void WriteLineColor(string text, ConsoleColor color)
//    {
//        var currentColor = Console.ForegroundColor;
//        Console.ForegroundColor = color;
//        Console.WriteLine(text);
//        Console.ForegroundColor = currentColor;
//    }

//    public Task SendMessageAsync(long id, string v, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }
//}
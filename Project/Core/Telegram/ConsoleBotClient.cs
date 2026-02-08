//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading;
////using System.Threading.Tasks;
//using Telegram.Bot;
//using Telegram.Bot.Exceptions;
//using Telegram.Bot.Polling;
//using Telegram.Bot.Requests.Abstractions;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.Payments;
//using Telegram.Bot.Types.ReplyMarkups;

//public class ConsoleBotClient : ITelegramBotClient
//{
//    public bool LocalBotServer => false;
//    public long BotId => 1;
//    public TimeSpan Timeout { get; set; }

//    public event Func<ITelegramBotClient, Exception, HandleErrorSource, CancellationToken, Task>? OnMakingApiRequest;
//    public event Func<ITelegramBotClient, ApiResponse, CancellationToken, Task>? OnApiResponseReceived;

//    public Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<Message> SendMessage(
//        ChatId chatId,
//        string text,
//        int? messageThreadId = null,
//        ParseMode? parseMode = null,
//        IEnumerable<MessageEntity>? entities = null,
//        LinkPreviewOptions? linkPreviewOptions = null,
//        bool? disableNotification = null,
//        bool? protectContent = null,
//        string? messageEffectId = null,
//        ReplyParameters? replyParameters = null,
//        IReplyMarkup? replyMarkup = null,
//        string? businessConnectionId = null,
//        CancellationToken cancellationToken = default)
//    {
//        WriteLineColor($"\n📤 Ответ бота:", ConsoleColor.Blue);
//        WriteLineColor(text, ConsoleColor.Cyan);

//        // Показываем клавиатуру если есть
//        if (replyMarkup != null && replyMarkup is ReplyKeyboardMarkup keyboard)
//        {
//            WriteLineColor("\n⌨️ Доступные кнопки:", ConsoleColor.Yellow);
//            foreach (var row in keyboard.Keyboard)
//            {
//                foreach (var button in row)
//                {
//                    Console.Write($"[{button.Text}] ");
//                }
//                Console.WriteLine();
//            }
//        }

//        return Task.FromResult(new Message());
//    }

//    private static void WriteLineColor(string text, ConsoleColor color)
//    {
//        var currentColor = Console.ForegroundColor;
//        Console.ForegroundColor = color;
//        Console.WriteLine(text);
//        Console.ForegroundColor = currentColor;
//    }

//    // Обязательные методы интерфейса
//    public Task<User> GetMe(CancellationToken cancellationToken = default)
//        => Task.FromResult(new User { Id = 1, IsBot = true, FirstName = "ConsoleBot" });

//    public Task<bool> TestApi(CancellationToken cancellationToken = default)
//        => Task.FromResult(true);

//    public Task DownloadFile(string filePath, Stream destination, CancellationToken cancellationToken = default)
//        => throw new NotImplementedException();

//    public Task<File> GetFile(string fileId, CancellationToken cancellationToken = default)
//        => throw new NotImplementedException();
//}
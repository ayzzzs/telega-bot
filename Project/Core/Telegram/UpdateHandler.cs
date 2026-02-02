using System;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;


public class UpdateHandler : Telegram.Bot.Polling.IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _todoService;
    private readonly IToDoReportService _reportService;

    public UpdateHandler(
        IUserService userService,
        IToDoService todoService,
        IToDoReportService reportService)
    {
        _userService = userService;
        _todoService = todoService;
        _reportService = reportService;
    }

    public Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Update received");
        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);
        return Task.CompletedTask;
    }
}
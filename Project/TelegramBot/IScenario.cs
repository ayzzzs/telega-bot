using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace bbbbb.Project.TelegramBot.Scenarios
{
    public interface IScenario
    {
        bool CanHandle(ScenarioType scenario);
        Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct);
        Task<ScenarioResult> HandleCallbackAsync(ITelegramBotClient bot, ScenarioContext context, CallbackQuery callbackQuery, CancellationToken ct);
    }
}
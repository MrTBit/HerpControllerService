using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HerpControllerService.Services;

public class TelegramService
{
    private readonly TelegramBotClient _bot;
    private readonly ChatId _chatId;

    private readonly AlertService _alertService;

    public TelegramService(IConfiguration configuration, AlertService alertService)
    {
        _bot = new(configuration["Telegram:BotToken"]!);
        _chatId = new(Convert.ToInt64(configuration["Telegram:ChatId"]!));

        _bot.OnUpdate += OnUpdate;
    }
    
    public async Task SendAlertAsync(long alertId, string message)
    {
        await _bot.SendMessage(_chatId, message, ParseMode.Html, protectContent: true,
            replyMarkup: new InlineKeyboardMarkup().AddButton("Acknowledge", alertId.ToString()));
    }

    private async Task HandleCallback(CallbackQuery callbackQuery)
    {
        await _bot.AnswerCallbackQuery(callbackQuery.Id, "Acknowledged");

        await _alertService.AcknowledgeAlert(Convert.ToInt64(callbackQuery.Data));
    }
    
    private async Task OnUpdate(Update update)
    {
        switch (update)
        {
            case { CallbackQuery: { } callbackQuery }:
            {
                await HandleCallback(callbackQuery);
                break;
            }
        }
    }
}
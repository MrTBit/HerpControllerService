using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HerpControllerService.Services;

public class TelegramService
{
    private readonly TelegramBotClient _bot;
    private readonly ChatId _chatId;

    private readonly IServiceProvider _services;

    public TelegramService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _services = serviceProvider;
        _bot = new(configuration["Telegram:BotToken"]!);
        _chatId = new(Convert.ToInt64(configuration["Telegram:ChatId"]!));
        
        _bot.OnUpdate += OnUpdate;
    }
    
    public async Task SendAlertAsync(long alertId, string message, bool isReminder)
    {
        await _bot.SendMessage(_chatId, message, ParseMode.Html, protectContent: true,
            replyMarkup: isReminder ? null : new InlineKeyboardMarkup().AddButton("Acknowledge", alertId.ToString()));
    }

    private async Task HandleCallback(CallbackQuery callbackQuery)
    {
        await _bot.AnswerCallbackQuery(callbackQuery.Id, "Acknowledged");
        await _bot.EditMessageReplyMarkup(_chatId, callbackQuery.Message!.Id);

        using var services = _services.CreateScope();
        var alertService = services.ServiceProvider.GetRequiredService<AlertService>();
        await alertService.AcknowledgeAlert(Convert.ToInt64(callbackQuery.Data));
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
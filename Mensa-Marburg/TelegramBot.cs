using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Mensa_Marburg;

public class TelegramBot
{
    private TelegramBotClient botClient;
    private CancellationTokenSource cts;
    private Dictionary<string, Action<ITelegramBotClient, Update, CancellationToken>> dictionary;

    public TelegramBot()
    {
        botClient = new TelegramBotClient(Setting.Instance.TelegramBotToken);
        cts = new CancellationTokenSource();
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        dictionary = new Dictionary<string, Action<ITelegramBotClient, Update, CancellationToken>>()
        {
            ["/start"] = Start
        };
        
        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;
        var chatId = message.Chat.Id;
        if (Setting.Instance.AdminsIDs.Contains(chatId))
        {
            if (dictionary.ContainsKey(messageText))
                dictionary[messageText](botClient, update, cancellationToken);
        }
        else
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "unauthorized access\nyour chatID: " + chatId,
                cancellationToken: cancellationToken);
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    private void Start(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        
    }
}
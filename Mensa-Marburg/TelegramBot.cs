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
    
    public TelegramBot()
    {
        botClient = new TelegramBotClient(Setting.Instance.TelegramBotToken);
        cts = new CancellationTokenSource();
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        // botClient.StartReceiving(
        //     updateHandler: HandleUpdateAsync,
        //     pollingErrorHandler: HandlePollingErrorAsync,
        //     receiverOptions: receiverOptions,
        //     cancellationToken: cts.Token
        // );
    }
    
}
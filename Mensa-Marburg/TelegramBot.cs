using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Mensa_Marburg;

public class TelegramBot
{
    private TelegramBotClient _telegramBotClient;
    private CancellationTokenSource cts;
    private Dictionary<string, Action<ITelegramBotClient, Update, CancellationToken>> dictionary;
    private Action<ITelegramBotClient, Update, CancellationToken, string>? nonKeyboardAction;
    private Regex TelegramID = new Regex("^(-)?\\d{6,11}$");

    public TelegramBot()
    {
        _telegramBotClient = new TelegramBotClient(Setting.Instance.TelegramBotToken);
        cts = new CancellationTokenSource();
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        dictionary = new Dictionary<string, Action<ITelegramBotClient, Update, CancellationToken>>()
        {
            ["/start"] = Start,
            ["Admin"] = Admin,
            ["Add Admin"] = AddAdmin,
            ["Remove Admin"] = RemoveAdmin,
            ["List Admin"] = ListAdmin
        };

        _telegramBotClient.StartReceiving(
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

    #region Actions

    private void Start(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Admin" },
            new KeyboardButton[] { "Get json dump" },
            new KeyboardButton[] { "Post to Channel" },
        })
        {
            ResizeKeyboard = true
        };
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Main Menu:",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    private void Admin(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "List Admin" },
            new KeyboardButton[] { "Add Admin" },
            new KeyboardButton[] { "Remove Admin" },
            new KeyboardButton[] { "Set Channel ID" },
        })
        {
            ResizeKeyboard = true
        };
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Admin Menu:",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    private void AddAdmin(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Enter new Admins ID",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
        nonKeyboardAction = (botClient, update, cancellationToken, messageText) =>
        {
            if (TelegramID.IsMatch(messageText))
            {
                Setting.Instance.AdminsIDs.Add(long.Parse(messageText));
                Setting.SaveSetting();
                nonKeyboardAction = null;
                botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Done",
                    cancellationToken: cancellationToken);
                Start(botClient, update, cancellationToken);
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "The new user Id is invalid",
                    cancellationToken: cancellationToken);
            }
        };
    }

    private void ListAdmin(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "List Admin" },
            new KeyboardButton[] { "Add Admin" },
            new KeyboardButton[] { "Remove Admin" },
            new KeyboardButton[] { "Set Channel ID" },
        })
        {
            ResizeKeyboard = true
        };
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "List of Admins:\n" + string.Join(", ", Setting.Instance.AdminsIDs),
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    private void RemoveAdmin(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Enter Admins ID to remove:\n" + string.Join(", ", Setting.Instance.AdminsIDs),
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
        nonKeyboardAction = (botClient, update, cancellationToken, messageText) =>
        {
            if (TelegramID.IsMatch(messageText))
            {
                var id = long.Parse(messageText);
                if (Setting.Instance.AdminsIDs.Contains(id))
                {
                    Setting.Instance.AdminsIDs.Remove(id);
                    Setting.SaveSetting();
                    nonKeyboardAction = null;
                    botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Done",
                        cancellationToken: cancellationToken);
                    Start(botClient, update, cancellationToken);
                }
                else
                {
                    botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "The user is not Admin",
                        cancellationToken: cancellationToken);
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "The user Id is invalid",
                    cancellationToken: cancellationToken);
            }
        };
    }

    #endregion
}
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
    public static readonly TelegramBot Instance;
    private TelegramBotClient _telegramBotClient;
    private CancellationTokenSource cts;
    private Dictionary<string, Action<ITelegramBotClient, Update, CancellationToken>> dictionary;
    private Action<ITelegramBotClient, Update, CancellationToken, string>? nonKeyboardAction;
    private Regex TelegramID = new Regex("^(-)?\\d{6,11}$");

    static TelegramBot()
    {
        Instance = new TelegramBot();
    }

    private TelegramBot()
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
            ["Back"] = Start,
            ["Admin"] = Admin,
            ["Add Admin"] = AddAdmin,
            ["Remove Admin"] = RemoveAdmin,
            ["List Admin"] = ListAdmin,
            ["Set Channel ID"] = SetChannelID,
            ["Channel"] = Channel,
            ["Post daily report to Channel"] = PostDailyToChannel,
            ["Post update report to Channel"] = PostUpdateToChannel,
            ["Post weekly report report to Channel"] = PostWeekReportToChannel,
            ["Auto send"] = AutoSend,
            ["Start Auto send"] = StartAutoSend,
            ["Stop Auto send"] = StopAutoSend,
            // [ "Stop daily report" ]
            // [ "Start daily report" ]
            // [ "Stop update report" ]
            // [ "Start update report" ]
            // [ "Stop weekly report report" ]
            // [ "Start weekly report report" ]
            ["Log"] = Log,
            ["Get today Export"] = GetTodayExport,
            ["Enable Log"] = EnableLog,
            ["Disable Log"] = DisableLog,
            ["Get json dump"] = GetJsonDump,
            ["Get Server Time"] = GetServerTime
        };

        _telegramBotClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );
    }

    public async void PostToChannel(string text)
    {
        await _telegramBotClient.SendTextMessageAsync(
            chatId: Setting.Instance.ChannelID,
            text: text,
            cancellationToken: cts.Token);
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
            else
                nonKeyboardAction?.Invoke(botClient, update, cancellationToken, messageText);
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
            new KeyboardButton[] { "Channel" },
            new KeyboardButton[] { "Log" },
            new KeyboardButton[] { "Get Server Time" }
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

    private void Channel(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Post daily report to Channel" },
            new KeyboardButton[] { "Post update report to Channel" },
            new KeyboardButton[] { "Post weekly report report to Channel" },
            new KeyboardButton[] { "Auto send" },
            new KeyboardButton[] { "Back" },
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

    private void AutoSend(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var autoSend = Setting.Instance.EnableService ? "Stop Auto send" : "Start Auto send";
        var dailySend = Setting.Instance.PostTage ? "Stop Auto send" : "Start Auto send";
        var updateSend = Setting.Instance.PostUpdate ? "Stop Auto send" : "Start Auto send";
        var weeklySend = Setting.Instance.PostWoche ? "Stop Auto send" : "Start Auto send";
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            // new KeyboardButton[] { "Stop Auto send" },
            // new KeyboardButton[] { "Start Auto send" },
            new KeyboardButton[] { autoSend },
            // new KeyboardButton[] { "Stop daily report" },
            // new KeyboardButton[] { "Start daily report" },
            new KeyboardButton[] { dailySend },
            // new KeyboardButton[] { "Stop update report" },
            // new KeyboardButton[] { "Start update report" },
            new KeyboardButton[] { updateSend },
            // new KeyboardButton[] { "Stop weekly report report" },
            // new KeyboardButton[] { "Start weekly report report" },
            new KeyboardButton[] { weeklySend },
            new KeyboardButton[] { "Back" },
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
            new KeyboardButton[] { "Back" },
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

    private void Log(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var enableLog = Setting.Instance.SaveLog ? "Disable Log" : "Enable Log";
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Get json dump" },
            new KeyboardButton[] { "Get today Export" },
            new KeyboardButton[] { enableLog },
            // new KeyboardButton[] { "Enable Log" },
            // new KeyboardButton[] { "Disable Log" },
            new KeyboardButton[] { "Back" },
        })
        {
            ResizeKeyboard = true
        };
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Log Menu:",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    private void StartAutoSend(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        Scheduler.Scheduler.Instance.ResumeSchedule();
        Thread.Sleep(1);
        Setting.Instance.EnableService = true;
        Setting.SaveSetting();
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Done",
            cancellationToken: cancellationToken);
        Start(botClient, update, cancellationToken);
    }

    private void StopAutoSend(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        Scheduler.Scheduler.Instance.PauseSchedule();
        Setting.Instance.EnableService = false;
        Setting.SaveSetting();
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Done",
            cancellationToken: cancellationToken);
        Start(botClient, update, cancellationToken);
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
                var id = long.Parse(messageText);
                if (Setting.Instance.AdminsIDs.Contains(id))
                {
                    nonKeyboardAction = null;
                    botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "The user is already Admin",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    Setting.Instance.AdminsIDs.Add(id);
                    Setting.SaveSetting();
                    nonKeyboardAction = null;
                    botClient.SendTextMessageAsync(
                        chatId: update.Message.Chat.Id,
                        text: "Done",
                        cancellationToken: cancellationToken);
                    Start(botClient, update, cancellationToken);
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

    private void SetChannelID(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Enter new Channel ID",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
        nonKeyboardAction = (botClient, update, cancellationToken, messageText) =>
        {
            if (TelegramID.IsMatch(messageText))
            {
                Setting.Instance.ChannelID = long.Parse(messageText);
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
                    text: "The user Id is invalid",
                    cancellationToken: cancellationToken);
            }
        };
    }

    private void PostDailyToChannel(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        Operator.Instance.Start(postTage: true);
    }

    private void PostUpdateToChannel(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        Operator.Instance.Start(postUpdate: true);
    }

    private void PostWeekReportToChannel(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        Operator.Instance.Start(postWoche: true);
    }

    private void GetJsonDump(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(Setting.WorkDir, "logs");
        var file = new DirectoryInfo(path).GetFiles().OrderBy(f => f.LastWriteTime).First();
        var name = file.Name.Replace("log.", "").Replace(".json", "");

        using Stream stream = System.IO.File.OpenRead(file.FullName);
        botClient.SendDocumentAsync(
            chatId: update.Message.Chat.Id,
            document: InputFile.FromStream(stream: stream, fileName: file.Name),
            caption: "log file",
            cancellationToken: cancellationToken);
    }

    private void GetTodayExport(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
    }

    private void EnableLog(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        Setting.Instance.SaveLog = true;
        Setting.SaveSetting();
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Done",
            cancellationToken: cancellationToken);
        Start(botClient, update, cancellationToken);
    }

    private void DisableLog(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        Setting.Instance.SaveLog = false;
        Setting.SaveSetting();
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: "Done",
            cancellationToken: cancellationToken);
        Start(botClient, update, cancellationToken);
    }

    private void GetServerTime(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        botClient.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            text: DateTime.Now.ToString("ddd dd.MM.yyyy : HH:mm"),
            cancellationToken: cancellationToken);
        Start(botClient, update, cancellationToken);
    }

    #endregion
}
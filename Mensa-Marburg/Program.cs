using System.Text.RegularExpressions;
using Mensa_Marburg;
using Mensa_Marburg.Scheduler;

Console.WriteLine("Starting");

if (Environment.GetEnvironmentVariable("init") == "true" ||
    Environment.GetCommandLineArgs().Contains("--init"))
    InitSetting();
else if (!Setting.LoadSetting())
{
    Setting.SetNewInstance();
    Setting.SaveSetting();
    Console.WriteLine("Setting File was created. Please fill it");
}
else if (Setting.Instance.IsSet)
{

    var bot = TelegramBot.Instance;
    var sc = Scheduler.Instance;
    var op = Operator.Instance;
    sc.Init();
    
    Console.WriteLine("fertig");
    if (Environment.GetCommandLineArgs().Contains("--in-docker"))
        await Task.Delay(Timeout.Infinite, new CancellationToken()).ConfigureAwait(false);
    else
        Console.ReadKey();
    sc.StopSchedule();
}
else
{
    Console.WriteLine("Setting File was created. Please fill Setting File");
}


void InitSetting()
{
    var telegramBotTokenRegex = new Regex("/^[0-9]{8,10}:[a-zA-Z0-9_-]{35}$/");
    var TelegramIDRegex = new Regex("^(-)?\\d{6,11}$");
    Setting.SetNewInstance();

    Console.WriteLine("Please Enter Telegram Bot Token");
    var tmp = Console.ReadLine();
    while (!telegramBotTokenRegex.IsMatch(tmp))
    {
        Console.WriteLine("Token is invalid.\nPlease Enter valid Telegram Bot Token");
        tmp = Console.ReadLine();
    }

    Setting.Instance.TelegramBotToken = tmp;

    Console.WriteLine("Please Enter Telegram user id of first Admin");
    tmp = Console.ReadLine();
    while (!TelegramIDRegex.IsMatch(tmp))
    {
        Console.WriteLine("user id is invalid.\nPlease Enter valid Telegram user id");
        tmp = Console.ReadLine();
    }

    Setting.Instance.AdminsIDs.Add(long.Parse(tmp));

    Console.WriteLine("Please Enter Telegram id of Channel");
    tmp = Console.ReadLine();
    while (!TelegramIDRegex.IsMatch(tmp))
    {
        Console.WriteLine("Channel id is invalid.\nPlease Enter valid Telegram Channel id");
        tmp = Console.ReadLine();
    }

    Setting.Instance.ChannelID = long.Parse(tmp);
    Setting.Instance.BaseURL = "https://studentenwerk-marburg.de/essen-trinken/speisekarte/";
    Setting.Instance.IsSet = true;
    Setting.Instance.SaveLog = true;
    Setting.Instance.TagCronString = "0 30 10 ? * MON-FRI"; // 10:30 am weekdays
    Setting.Instance.UpdateCronString = "0 20 14 ? * MON-FRI"; // 2:20 pm weekdays
    Setting.SaveSetting();
}
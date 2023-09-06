using Newtonsoft.Json;

namespace Mensa_Marburg;

public class Setting
{
    // I cant commit Telegram Bot Token of test bot
    public string TelegramBotToken { get; set; }
    public List<long> AdminsID { get; set; }
    public long ChannelID { get; set; }
    [JsonIgnore]
    private static string WorkDir = "./app"; // on docker it is /app

    public static void SaveDefaultSetting()
    {
        if (!Directory.Exists(WorkDir))
            Directory.CreateDirectory(WorkDir);
        using (var sw = new StreamWriter(Path.Combine(WorkDir, "setting.json")))
        {
            sw.Write(JsonConvert.SerializeObject(new Setting()));
        }
    }
}
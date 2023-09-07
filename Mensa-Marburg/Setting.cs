using Newtonsoft.Json;

namespace Mensa_Marburg;

public class Setting
{
    // I cant commit Telegram Bot Token of test bot
    public string TelegramBotToken { get; set; }
    public List<long> AdminsID { get; set; }
    public long ChannelID { get; set; }
    [JsonIgnore] private static string WorkDir = "./app"; // on docker it is /app
    [JsonIgnore] public static Setting Instance { get; private set; }

    private Setting()
    {
    }

    public static void SaveDefaultSetting()
    {
        if (!Directory.Exists(WorkDir))
            Directory.CreateDirectory(WorkDir);
        using (var sw = new StreamWriter(Path.Combine(WorkDir, "setting.json")))
        {
            Instance = new Setting();
            sw.Write(JsonConvert.SerializeObject(Instance, Formatting.Indented));
        }
    }

    public static Setting LoadDefaultSetting()
    {
        using var sr = new StreamReader(Path.Combine(WorkDir, "setting.json"));
        Instance = JsonConvert.DeserializeObject<Setting>(sr.ReadToEnd()) ??
                   throw new Exception("unable to load Setting");
        return Instance;
    }
}
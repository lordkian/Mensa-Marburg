using Newtonsoft.Json;

namespace Mensa_Marburg;

public class Setting
{
    // I cant commit Telegram Bot Token of test bot
    public string TelegramBotToken { get; set; }
    public List<long> AdminsIDs { get; private set; }
    public long ChannelID { get; set; }
    public string BaseURL { get; set; }
#if IN_DOCKER
// running inside docker
    [JsonIgnore] private static string WorkDir = "/data"; 
#else
    // running outside of docker
    [JsonIgnore] private static string WorkDir = "./data";
#endif
    [JsonIgnore] public static Setting Instance { get; private set; }

    private Setting()
    {
        AdminsIDs = new List<long>();
    }

    public static void SaveSetting()
    {
        if (!Directory.Exists(WorkDir))
            Directory.CreateDirectory(WorkDir);
        Instance ??= new Setting();
        using var sw = new StreamWriter(Path.Combine(WorkDir, "setting.json"));
        sw.Write(JsonConvert.SerializeObject(Instance, Formatting.Indented));
    }

    public static bool LoadSetting()
    {
        if (!File.Exists(Path.Combine(WorkDir, "setting.json")))
        {
            
            SaveSetting();
            return false;
        }
        using var sr = new StreamReader(Path.Combine(WorkDir, "setting.json"));
        Instance = JsonConvert.DeserializeObject<Setting>(sr.ReadToEnd()) ??
                   throw new Exception("unable to load Setting");
        return true;
    }
}
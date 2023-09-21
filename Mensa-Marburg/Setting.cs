using Newtonsoft.Json;

namespace Mensa_Marburg;

public class Setting
{
    // I cant commit Telegram Bot Token of test bot
    public string TelegramBotToken { get; set; }
    public List<long> AdminsIDs { get; private set; }
    public long ChannelID { get; set; }
    public string BaseURL { get; set; }

    [JsonIgnore] public static readonly string WorkDir;

    [JsonIgnore] public static Setting Instance { get; private set; }

    static Setting()
    {
        if (Environment.GetCommandLineArgs().Contains("--in-docker"))
            WorkDir = "/data";
        else
            WorkDir = "./data";
    }

    private Setting()
    {
        AdminsIDs = new List<long>();
    }

    public static void SaveSetting()
    {
        if (!Directory.Exists(WorkDir))
            Directory.CreateDirectory(WorkDir);
        if (Instance == null)
            throw new Exception("Instance in null");
        using var sw = new StreamWriter(Path.Combine(WorkDir, "setting.json"));
        sw.Write(JsonConvert.SerializeObject(Instance, Formatting.Indented));
    }

    public static bool LoadSetting()
    {
        var file = Path.Combine(WorkDir, "setting.json");
        if (!File.Exists(file))
            return false;

        using var sr = new StreamReader(file);
        Instance = JsonConvert.DeserializeObject<Setting>(sr.ReadToEnd()) ??
                   throw new Exception("unable to load Setting");
        return true;
    }

    public static void SetNewInstance()
    {
        Instance ??= new Setting();
    }
}
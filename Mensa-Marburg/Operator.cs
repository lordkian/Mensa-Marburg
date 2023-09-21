using Mensa_Marburg.Data;
using Newtonsoft.Json;

namespace Mensa_Marburg;

public class Operator
{
    public SpeiseContainer CurrentSpeiseContainer { get; private set; }
    public static readonly Operator Instance;

    static Operator()
    {
        Instance = new Operator();
    }

    private Operator()
    {
    }

    public void LoadSpeiseContainer()
    {
        var dateStr = DateTime.Now.ToString("yyyy.MM.dd.HH");
        CurrentSpeiseContainer = new SpeiseContainer();
        CurrentSpeiseContainer.DownloadData();
        CurrentSpeiseContainer.DownloadTime = DateTime.Now;

        var dirPath = Path.Combine(Setting.WorkDir, "logs");
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        var sw = new StreamWriter(Path.Combine(dirPath,
            "log." + dateStr + ".json"));
        sw.WriteLine(JsonConvert.SerializeObject(CurrentSpeiseContainer));
        sw.Close();
    }

    public void PostToChannel(bool mittagUpdate)
    {
        if (CurrentSpeiseContainer == null)
            throw new Exception("SpeiseContainer is null");
        if ((CurrentSpeiseContainer.DownloadTime - DateTime.Now).Hours > 0)
            throw new Exception("SpeiseContainer is too old");

        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var todayFoods = (
            from s in CurrentSpeiseContainer.Gerichte
            where s.Date == today
            select s
        ).ToList();

        var text = mittagUpdate ? "Today Menu\n" : "Today Menu (update)\n";;
     
        foreach (var item in todayFoods)
        {
            text += $"{item.EssenType}: {item.Name} ({item.Kosten})\nKennzeichnungen: ";
            foreach (var keypair in item.Kennzeichnungen)
                text += keypair.Key + ") " + keypair.Value + " ";
            text += "\n\n";
        }

        foreach (var item in CurrentSpeiseContainer.Beilagen)
        {
            text += $"{item.Type}: {item.Name}\nKennzeichnungen: ";
            foreach (var keypair in item.Kennzeichnungen)
                text += keypair.Key + ") " + keypair.Value + " ";
            text += "\n\n";
        }

        TelegramBot.Instance.PostToChannel(text);
    }

    public void PostToWochePlanChannel()
    {
        if (CurrentSpeiseContainer == null)
            throw new Exception("SpeiseContainer is null");
        if ((CurrentSpeiseContainer.DownloadTime - DateTime.Now).Hours > 0)
            throw new Exception("SpeiseContainer is too old");
        
      //  var text = "This Week"
        
    }
}
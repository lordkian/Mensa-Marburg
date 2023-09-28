using System.Globalization;
using System.Text.RegularExpressions;
using Mensa_Marburg.Data;
using Mensa_Marburg.Data.DataType;
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

    public void Start(bool postTage = true, bool postUpdate = true, bool postWoche = true)
    {
        // load infos
        var sp = new SpeiseContainer();
        sp.DownloadData();
        sp.DownloadTime = DateTime.Now;
        
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
            LoadSpeiseContainer();

        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var todayFoods = (
            from s in CurrentSpeiseContainer.Gerichte
            where s.Date == today
            select s
        ).ToList();

        var text = mittagUpdate ? "Today Menu (update)\n" : "Today Menu\n";

        foreach (var item in todayFoods)
        {
            text += $"{item.EssenType}: {item.Name} ({item.Kosten})\n";
            foreach (var itemSubGericht in item.SubGerichte)
                text += $"{itemSubGericht.Mensa} : {itemSubGericht.MenuArt}, ";

            text += "Kennzeichnungen: ";
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

    public void PostToWochePlanChannel(bool newMod = false)
    {
        if (CurrentSpeiseContainer == null)
            throw new Exception("SpeiseContainer is null");
        if ((CurrentSpeiseContainer.DownloadTime - DateTime.Now).Hours > 0)
            LoadSpeiseContainer();

        var text = "This Week Menu:\n\n";
        var grouped = (from gericht in CurrentSpeiseContainer.Gerichte
            group gericht by gericht.Date
            into myGroup
            select myGroup);
        var dic = new SortedDictionary<DateTime, List<Gericht>>();
        foreach (var itemGrouped in grouped)
        {
            var date = DateTime.ParseExact(itemGrouped.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            dic[date] = new List<Gericht>();
            dic[date].AddRange(itemGrouped);
        }

        var myRegex = new Regex("\\s*\\([^)]*\\)\\s*");
        var space = new Regex("\\s+");
        foreach (var itemList in dic.Keys)
        {
            text += itemList.ToString("ddd, MM.dd") + ":\n\n";
            foreach (var item in dic[itemList])
            {
                var cleanText = myRegex.Replace(item.Name, " ");
                cleanText = space.Replace(cleanText, " ");
                text += $"{item.EssenType}: {cleanText} ({item.Kosten})\n";
            }
        }

        TelegramBot.Instance.PostToChannel(text);
    }
}
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

        // save logs
        if (Setting.Instance.SaveLog)
            SaveLog(sp);

        // post Woche Report
        if (postWoche && DateTime.Now.DayOfWeek == DayOfWeek.Monday)
            WocheReport(sp);

        // remove unnecessary date
        sp.Clean();

        // post Tag report
        if (postTage && DateTime.Now.Hour >= 14)
            TagReport(sp);
        
        // post Tag Update
        if (postUpdate && DateTime.Now.Hour < 14)
            TagUpdateReport(sp);
    }

    #region privateMethods

    private static void SaveLog(SpeiseContainer sp)
    {
        var dateStr = DateTime.Now.ToString("yyyy.MM.dd.HH");
        var dirPath = Path.Combine(Setting.WorkDir, "logs");
        var name = Path.Combine(dirPath, "log." + dateStr + ".json");
        var i = 0;
        while (File.Exists(name))
            name = Path.Combine(dirPath, "log." + dateStr + "." + ++i + ".json");
        using var sw = new StreamWriter(name);
        sw.WriteLine(JsonConvert.SerializeObject(sp));
    }

    private static void WocheReport(SpeiseContainer sp)
    {
        TelegramBot.Instance.PostToChannel(sp.ToString(MessageStat.Woche));
    }

    private static void TagReport(SpeiseContainer sp)
    {
        TelegramBot.Instance.PostToChannel(
            "Today Menu\n" + sp.ToString(MessageStat.Tage));
    }
    private static void TagUpdateReport(SpeiseContainer sp)
    {
        TelegramBot.Instance.PostToChannel(
            "Today Menu (update)\n" + sp.ToString(MessageStat.Tage));
    }

    #endregion

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
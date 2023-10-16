using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Mensa_Marburg.Data.DataType;
using Newtonsoft.Json;

namespace Mensa_Marburg.Data;

public class SpeiseContainer
{
    public List<Gericht> Gerichte { get; private set; }
    public List<Gericht> GerichteTmp { get; private set; }
    public List<Beilage> Beilagen { get; private set; }
    public Dictionary<string, string> EssenTypeDic { get; private set; }
    public Dictionary<string, string> MensaDic { get; private set; }
    public Dictionary<string, string> GenerellKennzeichnungen { get; private set; }
    public DateTime DownloadTime { get; set; }
    [JsonIgnore] public string HTML { get; private set; }
    [JsonIgnore] private Regex CleanWhiteSpace = new Regex("\\s+");
    [JsonIgnore] private Regex KlammernRegex = new Regex("\\s*\\([^)]*\\)\\s*");

    #region ToString

    public string ToString(MessageStat stat)
    {
        string text = "";
        switch (stat)
        {
            case MessageStat.Tage:
                text = TageToString();
                break;
            case MessageStat.TageEmoji:
                text = TageToStringEmoji();
                break;
            case MessageStat.Woche:
                text = "This Week Menu:\n";
                text += WocheToString();
                break;
            case MessageStat.WocheEmoji:
                text = "This Week Menu:\n";
                text += WocheToStringEmoji();
                break;
            default:
                text = ToString();
                break;
        }

        return text;
    }

    private string TageToString()
    {
        var text = "";
        foreach (var item in Gerichte)
        {
            var cleanText = KlammernRegex.Replace(item.Name, " ");
            cleanText = CleanWhiteSpace.Replace(cleanText, " ");
            text += $"{item.EssenType}: {cleanText} ({item.Kosten}):\n";
            var joined = from sg in item.SubGerichte
                where !string.IsNullOrEmpty(sg.Mensa.Trim())
                select sg.Mensa;
            text += string.Join(", ", joined) + "\n\n";
            // foreach (var itemSubGericht in item.SubGerichte)
            //     text += $"{itemSubGericht.Mensa} : {itemSubGericht.MenuArt}\n";
            /* text += "Kennzeichnungen: ";
                    foreach (var keypair in item.Kennzeichnungen)
                        text += keypair.Key + ") " + keypair.Value + " ";
                    text += "\n\n";*/
        }

        var grouped = from beilage in Beilagen
            group beilage by beilage.Type
            into beilageType
            select beilageType;
        foreach (var groupedItem in grouped)
        {
            text += groupedItem.Key + ":\n";
            var tmp = new List<string>();
            foreach (var item in groupedItem)
            {
                var cleanText = KlammernRegex.Replace(item.Name, " ");
                cleanText = CleanWhiteSpace.Replace(cleanText, " ");
                tmp.Add(cleanText);
            }

            text += string.Join(" , ", tmp) + "\n";
        }
        /*foreach (var item in Beilagen)
        {
            var cleanText = KlammernRegex.Replace(item.Name, " ");
            cleanText = CleanWhiteSpace.Replace(cleanText, " ");
            text += $"{item.Type}: {cleanText}\n";
            // foreach (var keypair in item.Kennzeichnungen)
            //            text += keypair.Key + ") " + keypair.Value + " ";
            //        text += "\n\n";
        }*/

        return text;
    }

    private string TageToStringEmoji()
    {
        var text = "";
        foreach (var item in Gerichte)
        {
            var cleanText = KlammernRegex.Replace(item.Name, " ");
            cleanText = CleanWhiteSpace.Replace(cleanText, " ");
            var essenTypes = item.EssenType.Split("/").Where(s => s.Length > 1).ToList();
            var essenType = string.Join("\u2795", from et in essenTypes select EssenTypeToString(et));
            text += $"{essenType} <code><b><u>{cleanText}</u></b></code> {item.Kosten}:\n";
            // var joined = from sg in item.SubGerichte
            //     select sg.Mensa;
            // text += string.Join(", ", joined) + "\n";
            foreach (var itemSubGericht in item.SubGerichte)
                text += $"\ud83c\udf7d {itemSubGericht.Mensa} ";
            //    text += $"{itemSubGericht.Mensa} : {itemSubGericht.MenuArt}\n";
            text += "\n\n";
            /* text += "Kennzeichnungen: ";
                    foreach (var keypair in item.Kennzeichnungen)
                        text += keypair.Key + ") " + keypair.Value + " ";
                    text += "\n\n";*/
        }

        text += "<tg-spoiler>";
        var grouped = from beilage in Beilagen
            group beilage by beilage.Type
            into beilageType
            select beilageType;
        foreach (var groupedItem in grouped)
        {
            text += BeilageToString(groupedItem.Key) + ":\n";
            var tmp = new List<string>();
            foreach (var item in groupedItem)
            {
                var cleanText = KlammernRegex.Replace(item.Name, " ");
                cleanText = CleanWhiteSpace.Replace(cleanText, " ");
                tmp.Add(cleanText);
            }

            text += string.Join(" , ", tmp) + "\n";
        }

        text += "</tg-spoiler>";
        /*foreach (var item in Beilagen)
        {
            var cleanText = KlammernRegex.Replace(item.Name, " ");
            cleanText = CleanWhiteSpace.Replace(cleanText, " ");
            text += $"{item.Type}: {cleanText}\n";
            // foreach (var keypair in item.Kennzeichnungen)
            //            text += keypair.Key + ") " + keypair.Value + " ";
            //        text += "\n\n";
        }*/

        return text;
    }

    private string WocheToString()
    {
        var text = "";
        var grouped = (from gericht in Gerichte
            where DatesAreInTheSameWeek(gericht.DateTime, DateTime.Now)
            group gericht by gericht.Date
            into myGroup
            select myGroup);

        var dic = new SortedDictionary<DateTime, List<Gericht>>();
        foreach (var itemGrouped in grouped)
        {
            var date = itemGrouped.First().DateTime;
            dic[date] = new List<Gericht>();
            dic[date].AddRange(itemGrouped);
        }

        foreach (var itemList in dic.Keys)
        {
            text += itemList.ToString("ddd, MM.dd") + ":\n";
            foreach (var item in dic[itemList])
            {
                var cleanText = KlammernRegex.Replace(item.Name, " ");
                cleanText = CleanWhiteSpace.Replace(cleanText, " ");
                text += $"{item.EssenType}: {cleanText} ({item.Kosten})\n";
            }

            text += "\n";
        }

        return text;
    }

    private string WocheToStringEmoji()
    {
        var text = "";
        var grouped = (from gericht in Gerichte
            where DatesAreInTheSameWeek(gericht.DateTime, DateTime.Now)
            group gericht by gericht.Date
            into myGroup
            select myGroup);

        var dic = new SortedDictionary<DateTime, List<Gericht>>();
        foreach (var itemGrouped in grouped)
        {
            var date = itemGrouped.First().DateTime;
            dic[date] = new List<Gericht>();
            dic[date].AddRange(itemGrouped);
        }

        foreach (var itemList in dic.Keys)
        {
            text += itemList.ToString("ddd, MM.dd") + ":\n";
            foreach (var item in dic[itemList])
            {
                var cleanText = KlammernRegex.Replace(item.Name, " ");
                cleanText = CleanWhiteSpace.Replace(cleanText, " ");
                var essenTypes = item.EssenType.Split("/").Where(s => s.Length > 1).ToList();
                var essenType = string.Join("\u2795", from et in essenTypes select EssenTypeToString(et));
                text += $"{essenType} <code><b><u>{cleanText}</u></b></code> {item.Kosten}\n";
            }

            text += "\n";
        }

        return text;
    }

    private string EssenTypeToString(string EssenType)
    {
        // TODO temp solution 
        switch (EssenType)
        {
            case "Vegan":
                return "\ud83e\udd66 -vegan";
            case "Vegetarisch":
                return "\ud83e\udd66";
            case "Fisch":
                return "\ud83d\udc1f";
            case "Geflügel":
                return "\ud83d\udc14";
            case "Rind_Kalb":
                return "\ud83d\udc2e";
            case "Schwein":
                return "\ud83d\udc37";
            default:
                return "";
        }
    }

    private string BeilageToString(BeilageType beilageType)
    {
        switch (beilageType)
        {
            case BeilageType.Suppen:
                return "\ud83c\udf72";
            case BeilageType.warme_Speisen:
                return "\ud83c\udf5b";
            case BeilageType.Dessert:
                return "\ud83c\udf67";
            case BeilageType.Salat:
                return "\ud83e\udd57";
            case BeilageType.Unbekant:
                return "";
            default:
                return "";
        }
    }

    #endregion

    public void Clean()
    {
        GerichteTmp.Clear();
        HTML = "";
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        Gerichte.RemoveAll(g => g.Date != today);
    }

    public void DownloadData()
    {
        // load props
        Gerichte = new List<Gericht>();
        Beilagen = new List<Beilage>();
        GerichteTmp = new List<Gericht>();
        EssenTypeDic = new Dictionary<string, string>();
        MensaDic = new Dictionary<string, string>();
        GenerellKennzeichnungen = new Dictionary<string, string>();
        // load function vars
        // var loader = new HtmlWeb();
        // var doc = loader.Load(Setting.Instance.BaseURL);
        using (var client = new WebClient())
            HTML = client.DownloadString(Setting.Instance.BaseURL);

        var doc = new HtmlDocument();
        doc.LoadHtml(HTML);

        LoadDictionary(doc);
        LoadGerichte(doc);
        LoadBeilagen(doc);
        ParsaGerichte();
        LoadGenerellKennzeichnungen(doc);
    }

    #region privateMethods

    private void LoadDictionary(HtmlDocument doc)
    {
        // load Dics
        foreach (var item in doc.DocumentNode.SelectNodes("//select[@class='neo-menu-single-filter-type']/option"))
            if (item.Attributes.Contains("value"))
                EssenTypeDic[item.Attributes["value"].Value.Trim()] = item.InnerText.Replace(" / ", "_").Trim();
        foreach (var item in doc.DocumentNode.SelectNodes("//select[@class='neo-menu-single-canteens']/option"))
            if (item.Attributes.Contains("value"))
                MensaDic[item.Attributes["value"].Value.Trim()] = item.InnerText.Trim();
    }

    private void LoadGerichte(HtmlDocument doc)
    {
        // load Gerichte
        foreach (var item in doc.DocumentNode.SelectNodes("//div[@class='neo-menu-single-dishes']//tr[not(@data-day)]"))
        {
            // load vars
            var gericht = new Gericht();
            gericht.SubGerichte.Add(new SubGericht());
            // load Type2, Mensa & Date
            gericht.SubGerichte[0].Type2 = TryDo(() => item.Attributes["data-type2"].Value.Trim());
            gericht.SubGerichte[0].Mensa = TryDo(() => MensaDic[item.Attributes["data-canteen"].Value.Trim()]);
            gericht.Date = TryDo(() => item.Attributes["data-date"].Value.Trim());
            if (!string.IsNullOrEmpty(gericht.Date))
                gericht.DateTime = DateTime.ParseExact(gericht.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            // load EssenType
            var essenTypeTmp = TryDo(() => item.Attributes["data-type"].Value.Trim());
            if (essenTypeTmp == "")
                gericht.EssenType = "";
            else if (essenTypeTmp.Contains(" "))
                essenTypeTmp.Split(" ").ToList().ForEach(s => { gericht.EssenType += " / " + EssenTypeDic[s]; });
            else
                gericht.EssenType = EssenTypeDic[essenTypeTmp];
            // load Kosten & MenuArt
            gericht.Kosten = TryDo(() =>
                item.SelectSingleNode(".//td[@class=\"neo-menu-single-price\"]/span").InnerText.Trim());
            gericht.SubGerichte[0].MenuArt = TryDo(() =>
                item.SelectSingleNode(".//span[@class=\"neo-menu-single-type\"]").InnerText.Trim());

            // load Name & Kennzeichnungen
            var node = item.SelectSingleNode(".//span[@class=\"neo-menu-single-title\"]");
            gericht.Name = node.InnerText.Trim();
            var kennzeichnungenNode = node.SelectNodes(".//abbr");
            if (kennzeichnungenNode is { Count: > 0 })
                foreach (var item2 in kennzeichnungenNode)
                {
                    gericht.Kennzeichnungen.TryAdd(item2.InnerText.Trim(), item2.Attributes["title"].Value.Trim());
                }

            GerichteTmp.Add(gericht);
        }
    }

    private void LoadBeilagen(HtmlDocument doc)
    {
        // load Beilagen
        foreach (var item in doc.DocumentNode.SelectNodes("//div[@class=\"neo-module-inner modal-content\"]"))
        {
            // find BeilageType
            var beilageTypeText = item.SelectSingleNode("./h2").InnerText.Trim();
            BeilageType beilageTypeEnum;
            switch (beilageTypeText)
            {
                case "SUPPEN":
                    beilageTypeEnum = BeilageType.Suppen;
                    break;
                case "WARME SPEISEN":
                    beilageTypeEnum = BeilageType.warme_Speisen;
                    break;
                case "DESSERT":
                    beilageTypeEnum = BeilageType.Dessert;
                    break;
                case "SALAT":
                    beilageTypeEnum = BeilageType.Salat;
                    break;
                default:
                    beilageTypeEnum = BeilageType.Unbekant;
                    break;
            }

            // load Beilage infos
            foreach (var item2 in item.SelectNodes("./div/table"))
            {
                var beilage = new Beilage() { Type = beilageTypeEnum };
                var node = item.SelectSingleNode(".//span[@class=\"neo-menu-single-title\"]");
                beilage.Name = node.InnerText.Trim();
                var kennzeichnungenNode = node.SelectNodes(".//abbr");
                if (kennzeichnungenNode is { Count: > 0 })
                    foreach (var item3 in kennzeichnungenNode)
                    {
                        beilage.Kennzeichnungen.TryAdd(item3.InnerText.Trim(), item3.Attributes["title"].Value.Trim());
                    }

                Beilagen.Add(beilage);
            }
        }
    }

    private void ParsaGerichte()
    {
        // Parsa Gerichte
        var grouped = (from g in GerichteTmp
            group g by g.HashString
            into newGroup
            select newGroup);
        foreach (var item in grouped)
        {
            // collect common Data
            var first = item.First();
            var gerichtTmp = new Gericht();
            gerichtTmp.Name = item.First().Name;
            gerichtTmp.EssenType = item.First().EssenType;
            gerichtTmp.Date = item.First().Date;
            gerichtTmp.DateTime = item.First().DateTime;
            if (String.IsNullOrEmpty(gerichtTmp.Name) || String.IsNullOrEmpty(gerichtTmp.EssenType) ||
                String.IsNullOrEmpty(gerichtTmp.Date))
                continue;
            try
            {
                gerichtTmp.Kosten = item.First(g => !string.IsNullOrEmpty(g.Kosten) && !g.Kosten.Contains("0,00"))
                    .Kosten;
            }
            catch (Exception e)
            {
                gerichtTmp.Kosten = "-";
            }

            // combine other data
            foreach (var item2 in item)
            {
                gerichtTmp.SubGerichte.AddRange(item2.SubGerichte);
                foreach (var item3 in item2.Kennzeichnungen)
                    gerichtTmp.Kennzeichnungen.TryAdd(item3.Key, item3.Value);
            }

            Gerichte.Add(gerichtTmp);
        }
    }

    private void LoadGenerellKennzeichnungen(HtmlDocument doc)
    {
        // load GenerellKennzeichnungen
        foreach (var item in doc.DocumentNode.SelectNodes("//div[@class=\"neo-menu-single-additions\"]//li"))
        {
            var str = item.InnerText.Replace("&nbsp;", " ").Trim();
            str = CleanWhiteSpace.Replace(str, " ");
            var arr = str.Split(")");
            GenerellKennzeichnungen.TryAdd(arr[0], arr[1]);
        }
    }

    private bool DatesAreInTheSameWeek(DateTime date1, DateTime date2)
    {
        try
        {
            var cal = DateTimeFormatInfo.CurrentInfo.Calendar;
            var d1 = date1.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date1));
            var d2 = date2.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date2));

            return d1 == d2;
        }
        catch (Exception e)
        {
            Console.WriteLine("here");
            Console.WriteLine(e);
            throw;
        }
    }

    #endregion

    private static string TryDo(Func<string> func)
    {
        try
        {
            return func();
        }
        catch (Exception e)
        {
            return "";
        }
    }
}

public enum MessageStat
{
    Tage,
    TageEmoji,
    Woche,
    WocheEmoji
}
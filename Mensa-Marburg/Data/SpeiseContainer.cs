using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Mensa_Marburg.Data.DataType;

namespace Mensa_Marburg.Data;

public class SpeiseContainer
{
    public List<Gericht> Gerichte { get; private set; }
    public List<Gericht> GerichteTmp { get; private set; }
    public List<Beilage> Beilagen { get; private set; }
    public Dictionary<string, string> EssenTypeDic { get; private set; }
    public Dictionary<string, string> MensaDic { get; private set; }
    public Dictionary<string,string> GenerellKennzeichnungen { get; private set; }
    private Regex CleanWhiteSpace = new Regex("\\s+");

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
        var loader = new HtmlWeb();
        var doc = loader.Load(Setting.Instance.BaseURL);
  
        LoadDictionary(doc);
        LoadGerichte(doc);
        LoadBeilagen(doc);
        ParsaGerichte();
        LoadGenerellKennzeichnungen(doc);
    }

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
                foreach (var item3 in node.SelectNodes(".//abbr"))
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
            if (String.IsNullOrEmpty(gerichtTmp.Name) || String.IsNullOrEmpty(gerichtTmp.EssenType) ||
                String.IsNullOrEmpty(gerichtTmp.Date))
                continue;
            gerichtTmp.Kosten = item.First(g => !string.IsNullOrEmpty(g.Kosten) && !g.Kosten.Contains("0,00")).Kosten;
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
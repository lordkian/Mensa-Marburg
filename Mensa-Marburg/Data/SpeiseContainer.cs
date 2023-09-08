using HtmlAgilityPack;
using Mensa_Marburg.Data.DataType;

namespace Mensa_Marburg.Data;

public class SpeiseContainer
{
    public List<Gericht> Gerichte { get; private set; }
    public List<Gericht> ParsedGerichte { get; private set; }
    public List<Beilage> Beilagen { get; private set; }
    public Dictionary<string, string> EssenTypeDic { get; private set; }
    public Dictionary<string, string> MensaDic { get; private set; }

    public void DownloadData()
    {
        // load props
        Gerichte = new List<Gericht>();
        ParsedGerichte = new List<Gericht>();
        Beilagen = new List<Beilage>();
        EssenTypeDic = new Dictionary<string, string>();
        MensaDic = new Dictionary<string, string>();
        // load function vars
        var loader = new HtmlWeb();
        var doc = loader.Load(Setting.Instance.BaseURL);
        // load Dics
        foreach (var item in doc.DocumentNode.SelectNodes("//select[@class='neo-menu-single-filter-type']/option"))
            if (item.Attributes.Contains("value"))
                EssenTypeDic[item.Attributes["value"].Value.Trim()] = item.InnerText.Replace(" / ", "_").Trim();
        foreach (var item in doc.DocumentNode.SelectNodes("//select[@class='neo-menu-single-canteens']/option"))
            if (item.Attributes.Contains("value"))
                MensaDic[item.Attributes["value"].Value.Trim()] = item.InnerText.Trim();
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
                    gericht.Kennzeichnungen.TryAdd(item2.InnerText, item2.Attributes["title"].Value.Trim());
                }

            Gerichte.Add(gericht);
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
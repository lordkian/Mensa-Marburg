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
       
    }
}
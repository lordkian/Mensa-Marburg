namespace Mensa_Marburg.Data.DataType;

public class Gericht : Speise
{
    public string EssenType { get; set; }
    public string Date { get; set; }
    public string Kosten { get; set; }
    public List<SubGericht> SubGerichte { get; set; }

    public Gericht()
    {
        SubGerichte = new List<SubGericht>();
    }
    
    public string HashString => $"{EssenType}{Date}{Name}";
}

public class SubGericht
{
    public string Mensa { get; set; }
    public string Type2 { get; set; }
    public string MenuArt { get; set; }
}
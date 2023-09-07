namespace Mensa_Marburg.Data.DataType;

public abstract class Speise
{
    public string Name { get; set; }
    public Dictionary<string,string> Kennzeichnungen { get; private set; }

    public Speise()
    {
        Name = "";
        Kennzeichnungen = new Dictionary<string,string>();
    }
}
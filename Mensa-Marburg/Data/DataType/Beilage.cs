namespace Mensa_Marburg.Data.DataType;

public class Beilage : Speise
{
    public BeilageType Type { get; set; }
}

public enum BeilageType
{
    Suppen,
    warme_Speisen,
    Dessert,
    Salat,
    Unbekant
}
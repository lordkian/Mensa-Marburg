using Mensa_Marburg.Data.DataType;

namespace Mensa_Marburg.Data;

public class SpeiseContainer
{
    public List<Gericht> Gerichte { get; set; }
    public List<Gericht> ParsedGerichte { get; set; }
    public List<Beilage> Beilagen { get; set; }
    
}
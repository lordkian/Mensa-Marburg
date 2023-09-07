using Mensa_Marburg.Data.DataType;

namespace Mensa_Marburg.Data;

public class SpeiseContainer
{
    public List<Gericht> Gerichte { get; private set; }
    public List<Gericht> ParsedGerichte { get; private set; }
    public List<Beilage> Beilagen { get; private set; }
    public Dictionary<string, string> EssenTypeDic { get; private set; }
    public Dictionary<string, string> MensaDic { get; private set; }

   
}
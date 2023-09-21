using Mensa_Marburg.Data;

namespace Mensa_Marburg;

public class Operator
{
    public SpeiseContainer SpeiseContainer { get; set; }
    public static Operator Instance;

    static Operator()
    {
        Instance = new Operator();
    }
    
    private Operator()
    {
        
    }
}
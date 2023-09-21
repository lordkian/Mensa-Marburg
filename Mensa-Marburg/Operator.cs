using Mensa_Marburg.Data;
using Newtonsoft.Json;

namespace Mensa_Marburg;

public class Operator
{
    public SpeiseContainer CurrentSpeiseContainer { get; private set; }
    public TelegramBot Bot { get; set; }
    public Scheduler.Scheduler Scheduler { get; set; }
    public static Operator Instance;

    static Operator()
    {
        Instance = new Operator();
    }
    
    private Operator()
    {
        
    }
}
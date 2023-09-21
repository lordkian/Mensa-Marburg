using Mensa_Marburg;
using Mensa_Marburg.Data;
using Mensa_Marburg.Scheduler;
using Newtonsoft.Json;

Console.WriteLine("Starting");

if (Environment.GetEnvironmentVariable("init") == "true" || 
    Environment.GetCommandLineArgs().Contains("--init"))
    InitSetting();
else if (!Setting.LoadSetting())
{
    Setting.SetNewInstance();
    Setting.SaveSetting();
    Console.WriteLine("Setting File was created. Please fill it");
    return;
}
else
{
   
}


Console.WriteLine("fertig");
Console.ReadKey();

void InitSetting()
{
    Setting.SetNewInstance();
    
}
using Mensa_Marburg;

Console.WriteLine("Starting");

if (!Setting.LoadSetting())
{
    Console.WriteLine("Setting File was created. Please fill it");
    return;
}

Console.ReadKey();
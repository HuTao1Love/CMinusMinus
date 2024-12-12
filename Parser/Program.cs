using Newtonsoft.Json;

var text = File.ReadAllText(Console.ReadLine()!);

Console.WriteLine(JsonConvert.SerializeObject(text, new JsonSerializerSettings()
{
    Formatting = Formatting.Indented,
    TypeNameHandling = TypeNameHandling.All,
}));
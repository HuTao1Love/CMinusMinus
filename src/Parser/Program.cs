using Newtonsoft.Json;

var text = File.ReadAllText(Console.ReadLine()!);

var program = Parser.Parser.Parse(text);

Console.WriteLine(JsonConvert.SerializeObject(program, new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    TypeNameHandling = TypeNameHandling.All,
}));
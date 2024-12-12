using Antlr4.Runtime;
using Newtonsoft.Json;
using Parser;
using Parser.Grammar;

var file = Console.ReadLine();
var inputStream = new AntlrFileStream(file);
var lexer = new CmmLexer(inputStream);
var cts = new CommonTokenStream(lexer);
var parser = new CmmParser(cts);

var result = parser.program().Accept(new ElementBottomUpRewriter());
Console.WriteLine(JsonConvert.SerializeObject(result, new JsonSerializerSettings()
{
    Formatting = Formatting.Indented,
    TypeNameHandling = TypeNameHandling.All,
}));
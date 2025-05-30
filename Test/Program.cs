using Expressions.Models;
using Expressions.Visitors;
using Parser.Models;

var parser = new Parser.Models.Parser();
var context = new Context([], []);
var visit = new Execute(context);

// var path = Environment.CurrentDirectory;
var path = @"D:\";
path = Path.Combine(path, @"CodeText.pw");
var codeText = File.ReadAllText(path);

var tokens = Lexer.Tokenizer(codeText);
var node = parser.Parse(tokens);
node.Accept(visit);
 
// tokens = Lexer.Tokenizer(@"b= a + ""a"" == ""7a""");
// node = parser.Parse(tokens);
// node.Accept(visit);

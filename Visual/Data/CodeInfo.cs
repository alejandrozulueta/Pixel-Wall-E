using Expressions.Interfaces;
using Parser.Models;
using Core.Exceptions;
using ParserClass = Parser.Models.Parser;

namespace Visual.Data
{
    public class CodeInfo
    {
        public Tokens[] Tokens { get; set; }
        public Context Context { get; set; }
        public IInstruction Node { get; set; }
        public List<ExceptionWL> Exceptions { get; set; }

        public CodeInfo(Dictionary<string, FuncInfo> funcs, Dictionary<string, ActionInfo> acts, string code)
        {
            Exceptions = [];

            var parserObj = new ParserClass();
            Tokens = Lexer.Tokenizer(code, out List<ExceptionWL> exceptions);

            Exceptions.AddRange(exceptions);

            Context = new Context(funcs, acts);
            Node = parserObj.Parse(Tokens, out exceptions);

            Exceptions.AddRange(exceptions);
        }
    }
}
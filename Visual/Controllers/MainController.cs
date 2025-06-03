using Expressions.Interfaces;
using Expressions.Models;
using Expressions.Visitors;
using Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ParserClass = Parser.Models.Parser;

namespace Visual.Controllers
{
    public class MainController
    {
        public FuncControler FuncControler { get; }
        public ActionControler ActionControler { get; }

        Dictionary<string, FuncInfo> funcs;
        Dictionary<string, ActionInfo> acts;

        public MainController(FuncControler funcControler, ActionControler actionControler)
        {
            FuncControler = funcControler;
            ActionControler = actionControler;

            funcs = FuncControler.GetFuncs();
            acts = ActionControler.GetActs();
        }

        public void ExecuteCode(CodeInfo info)
        {
            var visit = new Execute(info.Context);
            info.Node.Accept(visit);
        }

        public bool TryCode(CodeInfo info, out List<Exception>? exceptions) 
        {
            if (info.Exceptions.Count != 0)
            {
                exceptions = info.Exceptions;
                return false;
            }
            var analyzer = new SemanticAnalyzer(info.Context);
            info.Node.Accept(analyzer);
            if (analyzer.GetExceptions(out exceptions))
                return true;
            return false;
        }
    }

    public class CodeInfo
    {
        public Tokens[] Tokens { get; set; }
        public Context Context { get; set; }
        public IInstruction Node { get; set; }
        public List<Exception> Exceptions { get; set; }

        public CodeInfo(Dictionary<string, FuncInfo> funcs, Dictionary<string, ActionInfo> acts, string code)
        {
            Exceptions = [];

            var parserObj = new ParserClass();
            Tokens = Lexer.Tokenizer(code, out List<Exception> exceptions);

            Exceptions.AddRange(exceptions);

            Context = new Context(funcs, acts);
            Node = parserObj.Parse(Tokens, out exceptions);

            Exceptions.AddRange(exceptions);
        }
    }
}
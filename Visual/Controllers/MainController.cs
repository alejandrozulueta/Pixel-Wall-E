using Expressions.Models;
using Expressions.Visitors;
using Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visual.Controllers
{
    public class MainController
    {
        public FuncControler FuncControler { get; }
        public ActionControler ActionControler { get; }
        
        public MainController(FuncControler funcControler, ActionControler actionControler)
        {
            FuncControler = funcControler;
            ActionControler = actionControler;
        }

        public void ExecuteCode(string code)
        {
            Dictionary<string, FuncInfo> funcs = FuncControler.GetFuncs();
            Dictionary<string, ActionInfo> acts = ActionControler.GetActs();

            var context = new Context(funcs, acts);
            var analyzer = new SemanticAnalyzer(context);
            var visit = new Execute(context);

            var tokens = Lexer.Tokenizer(code);
                
            var parser = new Parser.Models.Parser();

            
            var node = parser.Parse(tokens);

            node.Accept(analyzer);
            if(!analyzer.GetExceptions(out List<Exception>? exceptions)) 
            { 
                // Agregar Exc
                return;
            }

            node.Accept(visit);
        }
    }
}
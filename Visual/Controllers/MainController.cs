using Expressions.Models;
using Expressions.Visitors;
using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Visual.Data;

namespace Visual.Controllers
{
    public class MainController
    {
        public FuncControler FuncControler { get; }
        public ActionControler ActionControler { get; }

        private Dictionary<string, FuncInfo> funcs;
        private Dictionary<string, ActionInfo> acts;

        public MainController(FuncControler funcControler, ActionControler actionControler)
        {
            FuncControler = funcControler;
            ActionControler = actionControler;

            funcs = FuncControler.GetFuncs();
            acts = ActionControler.GetActs();
        }

        public void ExecuteCode(CodeInfo info)
        {
            ActionControler._exist = false;
            var visit = new Execute(info.Context);
            info.Node.Accept(visit);
        }

        public bool TryCode(CodeInfo info, out List<ExceptionWL>? exceptions) 
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
}
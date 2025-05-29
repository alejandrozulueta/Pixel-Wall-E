using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressions.Models
{
    public class Scope(Dictionary<string, Values> variables, Dictionary<string, int> labels, Scope? parent = null)
    {
        public Scope? Parent { get; protected set; } = parent;
        public Dictionary<string, Values> Variables { get; } = variables;
        public Dictionary<string, int> Labels { get; } = labels;

        public void Reset()
        {
            Variables.Clear();
            Labels.Clear();
        }

        public bool TryGetVariable(string name, out Values? value)
        {
            if (Variables.TryGetValue(name, out value))
                return true;
            if (parent is not null)
                return Parent!.TryGetVariable(name, out value);
            value = default;
            return false;
        }

        public bool TryGetLabel(string name, out int value)
        {
            if (Labels.TryGetValue(name, out value))
                return true;
            if (parent is not null)
                return Parent!.TryGetLabel(name, out value);
            return false;
        }
    }
}

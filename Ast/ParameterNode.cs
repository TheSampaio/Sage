using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Ast
{
    public class ParameterNode
    {
        public string Name { get; }
        public string Type { get; }
        public ParameterNode(string name, string type) { Name = name; Type = type; }
    }
}

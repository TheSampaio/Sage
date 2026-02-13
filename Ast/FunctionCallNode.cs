using Sage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Ast
{
    public class FunctionCallNode : AstNode
    {
        public string Name { get; }
        public List<AstNode> Args { get; }
        public FunctionCallNode(string n, List<AstNode> args) { Name = n; Args = args; }
        public override T Accept<T>(IAstVisitor<T> v) => v.Visit(this);
    }
}

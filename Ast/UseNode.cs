using Sage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Ast
{
    public class UseNode : AstNode
    {
        public string Module { get; }
        public UseNode(string m) => Module = m;
        public override T Accept<T>(IAstVisitor<T> v) => v.Visit(this);
    }
}

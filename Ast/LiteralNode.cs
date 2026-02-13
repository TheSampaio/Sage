using Sage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Ast
{
    public class LiteralNode : AstNode
    {
        public object Value { get; }
        public string TypeName { get; }
        public LiteralNode(object v, string t) { Value = v; TypeName = t; }
        public override T Accept<T>(IAstVisitor<T> v) => v.Visit(this);
    }
}

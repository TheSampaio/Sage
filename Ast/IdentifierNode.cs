using Sage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Ast
{
    public class IdentifierNode : AstNode
    {
        public string Name { get; }
        public IdentifierNode(string n) => Name = n;
        public override T Accept<T>(IAstVisitor<T> v) => v.Visit(this);
    }
}

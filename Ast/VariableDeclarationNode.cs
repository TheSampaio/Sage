using Sage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Ast
{
    public class VariableDeclarationNode : AstNode
    {
        public string Type { get; }
        public string Name { get; }
        public AstNode Initializer { get; }
        public VariableDeclarationNode(string t, string n, AstNode i) { Type = t; Name = n; Initializer = i; }
        public override T Accept<T>(IAstVisitor<T> v) => v.Visit(this);
    }
}

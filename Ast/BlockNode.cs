using Sage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Ast
{
    public class BlockNode : AstNode
    {
        public List<AstNode> Statements { get; } = new();
        public override T Accept<T>(IAstVisitor<T> v) => v.Visit(this);
    }
}

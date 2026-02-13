using Sage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Ast
{
    public class ExpressionStatementNode : AstNode
    {
        public AstNode Expression { get; }
        public ExpressionStatementNode(AstNode e) => Expression = e;
        public override T Accept<T>(IAstVisitor<T> v) => v.Visit(this);
    }
}

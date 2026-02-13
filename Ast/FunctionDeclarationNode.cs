using Sage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Ast
{
    public class FunctionDeclarationNode : AstNode
    {
        public string Name { get; }
        public string ReturnType { get; }
        public List<ParameterNode> Parameters { get; }
        public BlockNode Body { get; }
        public string ModuleOwner { get; set; } // Para saber se pertence a 'math'

        public FunctionDeclarationNode(string name, string returnType, List<ParameterNode> parameters, BlockNode body, string moduleOwner = "")
        {
            Name = name;
            ReturnType = returnType;
            Parameters = parameters;
            Body = body;
            ModuleOwner = moduleOwner;
        }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.Visit(this);
    }
}

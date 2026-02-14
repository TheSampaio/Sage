using Sage.Ast;

namespace Sage.Interfaces
{
    /// <summary>
    /// Defines the contract for AST traversal using the Visitor pattern.
    /// Each method corresponds to a specific node type in the Sage Abstract Syntax Tree.
    /// </summary>
    /// <typeparam name="T">The return type of the visit operations (e.g., string for code generation, bool for semantic analysis).</typeparam>
    public interface IAstVisitor<out T>
    {
        /// <summary>Visits the root node of the Sage program.</summary>
        T Visit(ProgramNode node);

        /// <summary>Visits a module/namespace definition.</summary>
        T Visit(ModuleNode node);

        /// <summary>Visits a function declaration including its signature and body.</summary>
        T Visit(FunctionDeclarationNode node);

        /// <summary>Visits a block of code enclosed in braces {}.</summary>
        T Visit(BlockNode node);

        /// <summary>Visits a variable declaration (e.g., name: type = value;).</summary>
        T Visit(VariableDeclarationNode node);

        /// <summary>Visits a return statement.</summary>
        T Visit(ReturnNode node);

        /// <summary>Visits a statement that consists of a single expression.</summary>
        T Visit(ExpressionStatementNode node);

        /// <summary>Visits a 'use' directive for importing external modules.</summary>
        T Visit(UseNode node);

        /// <summary>Visits a binary operation (e.g., addition, subtraction).</summary>
        T Visit(BinaryExpressionNode node);

        /// <summary>Visits a literal value (numbers, strings).</summary>
        T Visit(LiteralNode node);

        /// <summary>Visits an identifier (variable or function names).</summary>
        T Visit(IdentifierNode node);

        /// <summary>Visits a function call expression.</summary>
        T Visit(FunctionCallNode node);

        /// <summary>Visits a string node containing interpolation markers.</summary>
        T Visit(InterpolatedStringNode node);
    }
}
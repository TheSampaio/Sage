using Sage.Ast;

namespace Sage.Interfaces
{
    /// <summary>
    /// Defines a visitor for the Sage Abstract Syntax Tree (AST).
    /// Implements the Visitor Pattern to decouple node structures from their processing logic.
    /// </summary>
    /// <typeparam name="out T">The return type of the visit operations.</typeparam>
    public interface IAstVisitor<out T>
    {
        // --- Structural Nodes ---

        /// <summary>Visits the root node of the Sage program.</summary>
        T Visit(ProgramNode node);

        /// <summary>Visits a module or namespace definition.</summary>
        T Visit(ModuleNode node);

        /// <summary>Visits a function declaration including its signature and body.</summary>
        T Visit(FunctionDeclarationNode node);

        /// <summary>Visits a block of code enclosed in braces {}.</summary>
        T Visit(BlockNode node);

        // --- Statement Nodes ---

        /// <summary>Visits a variable or constant declaration.</summary>
        T Visit(VariableDeclarationNode node);

        /// <summary>Visits an assignment operation.</summary>
        T Visit(AssignmentNode node);

        /// <summary>Visits a return statement.</summary>
        T Visit(ReturnNode node);

        /// <summary>Visits a statement that consists of a standalone expression.</summary>
        T Visit(ExpressionStatementNode node);

        T Visit(ExternBlockNode node);

        /// <summary>Visits a 'use' directive for module imports.</summary>
        T Visit(UseNode node);

        // --- Control Flow Nodes ---

        /// <summary>Visits an if-else conditional branch.</summary>
        T Visit(IfNode node);

        /// <summary>Visits a while loop structure.</summary>
        T Visit(WhileNode node);

        /// <summary>Visits a for loop structure.</summary>
        T Visit(ForNode node);

        // --- Expression Nodes ---

        /// <summary>Visits a binary operation (e.g., +, -, &&, ==).</summary>
        T Visit(BinaryExpressionNode node);

        /// <summary>Visits a unary operation (e.g., !, -, ++).</summary>
        T Visit(UnaryExpressionNode node);

        /// <summary>Visits an explicit type cast (as).</summary>
        T Visit(CastExpressionNode node);

        /// <summary>Visits a function call expression.</summary>
        T Visit(FunctionCallNode node);

        // --- Literal & Leaf Nodes ---

        /// <summary>Visits a literal value (integer, float, boolean, string).</summary>
        T Visit(LiteralNode node);

        /// <summary>Visits an identifier (variable or function reference).</summary>
        T Visit(IdentifierNode node);

        /// <summary>Visits a string containing interpolation markers.</summary>
        T Visit(InterpolatedStringNode node);

        /// <summary>Visits a struct declaration.</summary>
        T Visit(StructDeclarationNode node);

        /// <summary>Visits a struct initialization.</summary>
        T Visit(StructInitializationNode node);
    }
}
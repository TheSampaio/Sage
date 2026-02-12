using Sage.Core.AST;

namespace Sage.Core
{
    public class Printer
    {
        public static void Print(AstNode node, string indent = "")
        {
            switch (node)
            {
                case ProgramNode program:
                    Console.WriteLine($"{indent}Program");
                    foreach (var stmt in program.Statements)
                        Print(stmt, indent + "  ");
                    break;

                case UseNode use:
                    Console.WriteLine($"{indent}Use Package: {use.ModuleName}");
                    break;

                case FunctionDeclarationNode func:
                    Console.WriteLine($"{indent}Function {func.Name} -> {func.ReturnType}");
                    Print(func.Body, indent + "  ");
                    break;

                case BlockNode block:
                    Console.WriteLine($"{indent}Block {{");
                    foreach (var stmt in block.Statements)
                        Print(stmt, indent + "    ");
                    Console.WriteLine($"{indent}}}");
                    break;

                case VariableDeclarationNode varDecl:
                    Console.WriteLine($"{indent}Var {varDecl.Name} ({varDecl.Type}) =");
                    Print(varDecl.Initializer, indent + "    ");
                    break;

                case ReturnNode ret:
                    Console.WriteLine($"{indent}Return");
                    if (ret.Expression != null)
                        Print(ret.Expression, indent + "    ");
                    break;

                case ExpressionStatementNode exprStmt:
                    Print(exprStmt.Expression, indent);
                    break;

                case BinaryExpressionNode bin:
                    Console.WriteLine($"{indent}BinaryOp ({bin.Operator})");
                    Print(bin.Left, indent + "  | Left: ");
                    Print(bin.Right, indent + "  | Right: ");
                    break;

                case FunctionCallNode call:
                    Console.WriteLine($"{indent}Call {call.FunctionName}");
                    foreach (var arg in call.Arguments)
                        Print(arg, indent + "    Arg: ");
                    break;

                case LiteralNode lit:
                    Console.WriteLine($"{indent}Literal ({lit.TypeName}): {lit.Value}");
                    break;

                case IdentifierNode id:
                    Console.WriteLine($"{indent}Id: {id.Name}");
                    break;

                case InterpolatedStringNode interpolated:
                    Console.WriteLine($"{indent}Interpolated String:");
                    foreach (var part in interpolated.Parts)
                        Print(part, indent + "  | ");
                    break;

                case ParenthesizedExpressionNode paren:
                    Console.WriteLine($"{indent}Group ( )");
                    Print(paren.Expression, indent + "  ");
                    break;

                default:
                    Console.WriteLine($"{indent}Unknown Node: {node.GetType().Name}");
                    break;
            }
        }
    }
}
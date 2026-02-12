using Sage.Core.AST;

namespace Sage.Core
{
    public class SemanticAnalyzer
    {
        private readonly SymbolTable _symbolTable;

        public SemanticAnalyzer()
        {
            _symbolTable = new SymbolTable();
        }

        public void Analyze(ProgramNode program)
        {
            foreach (var stmt in program.Statements)
            {
                Visit(stmt);
            }
        }

        private void Visit(AstNode node)
        {
            switch (node)
            {
                case FunctionDeclarationNode func:
                    // Define function in current scope (so it can be called recursively)
                    // TODO: In a real compiler, we check if function exists, signatures, etc.
                    // _symbolTable.Define(func.Name, "function"); 

                    _symbolTable.EnterScope(); // New scope for function body
                    Visit(func.Body);
                    _symbolTable.ExitScope();
                    break;

                case BlockNode block:
                    // Blocks inside functions usually don't need a new stack frame in C#, 
                    // but for scope isolation (like inside an 'if'), we do.
                    // For simplicity, let's treat blocks as transparent for now or add scope if needed.
                    foreach (var stmt in block.Statements)
                        Visit(stmt);
                    break;

                case VariableDeclarationNode varDecl:
                    Visit(varDecl.Initializer); // Check the expression on the right side first
                    _symbolTable.Define(varDecl.Name, varDecl.Type);
                    break;

                case ReturnNode ret:
                    if (ret.Expression != null)
                        Visit(ret.Expression);
                    break;

                case ExpressionStatementNode exprStmt:
                    Visit(exprStmt.Expression);
                    break;

                case BinaryExpressionNode bin:
                    Visit(bin.Left);
                    Visit(bin.Right);
                    // Here we would check types: e.g. if Left is string and Right is int
                    break;

                case FunctionCallNode call:
                    foreach (var arg in call.Arguments)
                        Visit(arg);
                    // Here we would check if function exists
                    break;

                case IdentifierNode id:
                    var symbol = _symbolTable.Resolve(id.Name);
                    if (symbol == null)
                    {
                        // Special case for Console (which is a module/namespace, not a variable)
                        if (id.Name.StartsWith("Console")) return;

                        throw new Exception($"[SEMANTIC ERROR] Variable '{id.Name}' has not been declared.");
                    }
                    break;

                case InterpolatedStringNode interpolated:
                    foreach (var part in interpolated.Parts)
                    {
                        Visit(part); // This will validate if variables like 'result' exist!
                    }
                    break;

                case ParenthesizedExpressionNode paren:
                    Visit(paren.Expression);
                    break;

                // Literals don't need validation
                case LiteralNode _:
                case UseNode _:
                    break;

                default:
                    throw new Exception($"[SEMANTIC ERROR] Unknown node type: {node.GetType().Name}");
            }
        }
    }
}
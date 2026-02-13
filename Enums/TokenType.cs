namespace Sage.Enums
{
    public enum TokenType
    {
        // Controle
        EndOfFile,
        Unknown,

        // Literais e Identificadores
        Identifier,     // Ex: main, console, result
        Number,         // Ex: 10, 3.14
        String,         // Ex: "Hello World"

        // Palavras-chave (Keywords)
        Keyword_Use,    // use
        Keyword_Module, // module
        Keyword_Func,   // func
        Keyword_Return, // return

        // Tipos Primitivos (Sage Types)
        Type_U8, Type_U16, Type_U32, Type_U64,
        Type_I8, Type_I16, Type_I32, Type_I64,
        Type_F32, Type_F64,
        Type_B8,        // bool
        Type_Char,      // c8
        Type_Str,       // string
        Type_Void,      // none

        // Operadores Matemáticos
        Plus,           // +
        Minus,          // -
        Asterisk,       // *
        Slash,          // /
        Equals,         // =

        // Pontuação e Símbolos
        OpenParen,      // (
        CloseParen,     // )
        OpenBrace,      // {
        CloseBrace,     // }
        Semicolon,      // ;
        Comma,          // ,
        Colon,          // : (Usado na definição de tipo: func sum(): i32)
        DoubleColon,    // :: (Usado em namespaces: console::print)
        Arrow           // -> (Mantido para compatibilidade futura, caso mude a sintaxe)
    }
}

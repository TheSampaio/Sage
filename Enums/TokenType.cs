namespace Sage.Enums
{
    public enum TokenType
    {
        EndOfFile,

        // Identifiers e Literals
        Identifier,     // Ex: Console, number01, Main
        Number,         // Ex: 5, 10, 3.14
        String,         // Ex: "Hello World"

        // Keywords
        Keyword_Use,    // use
        Keyword_Func,   // func
        Keyword_Return, // return

        // Primitive Types
        Type_U8, Type_U16, Type_U32, Type_U64,
        Type_I8, Type_I16, Type_I32, Type_I64,
        Type_F32, Type_F64,
        Type_B8,       // Boolean
        Type_Char,     // c8
        Type_Str,      // high-level string (std::string)
        Type_Void,     // none

        // Values
        Value_Null,    // null

        // Operators and Punctuation
        Plus,           // +
        Minus,          // -
        Asterisk,       // *
        Slash,          // /
        Equals,         // =

        Arrow,          // ->
        DoubleColon,    // ::

        OpenParen,      // (
        CloseParen,     // )
        OpenBrace,      // {
        CloseBrace,     // }
        Semicolon,      // ;
        Comma,          // ,

        // Others
        Unknown
    }
}
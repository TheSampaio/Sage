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

        // Primitive Types (Treated as keywords for now)
        Type_I32,       // i32

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
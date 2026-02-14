namespace Sage.Enums
{
    /// <summary>
    /// Categorizes the various types of symbols, keywords, and literals identified in Sage source code.
    /// </summary>
    public enum TokenType
    {
        // --- Control Tokens ---
        EndOfFile,
        Unknown,

        // --- Identifiers & Literals ---
        Identifier,
        Number,
        String,
        InterpolatedString, // Added for your InterpolatedStringNode logic

        // --- Keywords ---
        Keyword_Use,
        Keyword_Module,
        Keyword_Func,
        Keyword_Return,
        Keyword_Var, // Added for variable declarations

        // --- Data Types ---
        Type_I8, Type_I16, Type_I32, Type_I64,
        Type_U8, Type_U16, Type_U32, Type_U64,
        Type_F32, Type_F64,
        Type_B8,
        Type_C8, Type_C16, Type_C32,
        Type_Str,
        Type_Void,

        // --- Operators ---
        Plus,       // +
        Minus,      // -
        Asterisk,   // *
        Slash,      // /
        Equals,     // =
        Arrow,      // -> (Useful for function return signatures)

        // --- Punctuation & Delimiters ---
        OpenParen,   // (
        CloseParen,  // )
        OpenBrace,   // {
        CloseBrace,  // }
        Semicolon,   // ;
        Comma,       // ,
        Colon,       // :
        DoubleColon  // ::
    }
}
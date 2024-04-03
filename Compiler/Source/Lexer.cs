using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Sage
{
    enum Token
    {
        Integer = 0,
        Name,
        Number,
        Keyword,
        Operator,
        String
    }

    internal class Lexer
    {
        private StreamReader Reader;
        List<string> Tokens;

        public void Read(string fileName)
        {
            if (File.Exists(fileName))
            {
                Reader = new StreamReader(fileName);

                string line;

                while ((line = Reader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        // Remove commentaries
                        line = (line.Contains("//")) ? line.Substring(0, line.IndexOf("//")) : line;

                        line = Separate(line, ";");
                        line = Separate(line, "::");

                        line = Separate(line, "(");
                        line = Separate(line, ")");
                        line = Separate(line, "[");
                        line = Separate(line, "]");
                        line = Separate(line, "{");
                        line = Separate(line, "}");

                        // Tokenize the line
                        Tokens = line.Split(' ').ToList();

                        // Remove empty tokens
                        Tokens.RemoveAll(token => token == "");

                        foreach (string token in Tokens)
                            ClassifyToken(token);

                    }
                }
            }

            else
                Console.WriteLine($"[ERROR] Failed to read the file \"{fileName}\".");
        }

        private string Separate(string text, string newText)
        {
            if (text.Contains(newText))
                text = text.Replace(newText, $" {newText} ");

            return text;
        }

        private void ClassifyToken(string token)
        {
            // Remove any tabs from the token
            token = token.Replace("\t", "");

            // Check if the token is a keyword
            if (IsKeyword(token))
            {
                Console.WriteLine($"{{ Value: \"{token}\" | Type: {Token.Keyword} }}");
                return;
            }

            // Check if the token is an operator
            if (IsOperator(token))
            {
                Console.WriteLine($"{{ Value: \"{token}\" | Type: {Token.Operator} }}");
                return;
            }

            // Check if the token is a string
            if (IsString(token))
            {
                Console.WriteLine($"{{ Value: \"{token}\" | Type: {Token.String} }}");
                return;
            }

            // Check if the token is an integer
            if (IsInteger(token))
            {
                Console.WriteLine($"{{ Value: \"{token}\" | Type: {Token.Integer} }}");
                return;
            }

            // Check if the token is an integer
            if (IsNumber(token))
            {
                Console.WriteLine($"{{ Value: \"{token}\" | Type: {Token.Number} }}");
                return;
            }

            // Otherwise, consider the token as a name
            Console.WriteLine($"{{ Value: \"{token}\" | Type: {Token.Name} }}");
        }

        private bool IsKeyword(string token)
        {
            // List of keywords
            string[] keywords = { "function", "for", "if", "return", "use", "while" };
            return Array.Exists(keywords, k => k == token);
        }

        private bool IsOperator(string token)
        {
            // List of operators
            string[] operators = { "=", ";", "->", "(", ")", "[", "]", "{", "}" };
            return Array.Exists(operators, op => op == token);
        }

        private bool IsString(string token)
        {
            // Check if the token starts and ends with double quotes
            return token.StartsWith("\"") && token.EndsWith("\"");
        }

        private bool IsInteger(string token)
        {
            string[] integers = { "i8", "u8", "i16", "u16", "i32", "u32", "i64", "u64" };
            return Array.Exists(integers, k => k == token);
        }

        private bool IsNumber(string token)
        {
            string[] numbers = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            return Array.Exists(numbers, k => k == token);
        }
    }
}

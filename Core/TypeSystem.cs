namespace Sage.Core
{
    /// <summary>
    /// Centralized service for handling Sage type definitions, conversions, and compatibility rules.
    /// </summary>
    public static class TypeSystem
    {
        private static readonly List<string> NumericHierarchy =
            ["i8", "u8", "i16", "u16", "i32", "u32", "i64", "u64", "f32", "f64"];

        /// <summary>
        /// Converts a Sage type identifier to its corresponding C language type.
        /// Handles pointer recursion and special types like 'none' (void) or 'str' (char*).
        /// </summary>
        public static string ToCType(string sageType)
        {
            if (string.IsNullOrEmpty(sageType)) return "void";

            // Recursive pointer handling
            if (sageType.EndsWith("*"))
            {
                string baseType = sageType[..^1];
                return $"{ToCType(baseType)}*";
            }

            return sageType switch
            {
                "none" => "void",
                "str" => "char*",
                _ => sageType // Primitives (i32, f32, etc.) map directly to typedefs
            };
        }

        /// <summary>
        /// Determines if a type is a numeric primitive.
        /// </summary>
        public static bool IsNumeric(string type) => NumericHierarchy.Contains(type);

        /// <summary>
        /// Determines if a type is a floating-point primitive.
        /// </summary>
        public static bool IsFloatingPoint(string type) => type is "f32" or "f64";

        /// <summary>
        /// Determines the resulting type of a binary operation between two numeric types
        /// based on implicit promotion rules.
        /// </summary>
        public static string? GetDominantType(string typeA, string typeB)
        {
            int indexA = NumericHierarchy.IndexOf(typeA);
            int indexB = NumericHierarchy.IndexOf(typeB);

            if (indexA == -1 || indexB == -1) return null;
            return NumericHierarchy[Math.Max(indexA, indexB)];
        }

        /// <summary>
        /// Checks if a value of <paramref name="sourceType"/> can be implicitly assigned to <paramref name="targetType"/>.
        /// </summary>
        public static bool AreTypesCompatible(string targetType, string sourceType)
        {
            if (targetType == sourceType) return true;

            // Universal Pointer Compatibility: 'none*' (void*) can implicitly cast to/from any other pointer type (including 'str')
            bool isTargetPointer = targetType.EndsWith("*") || targetType == "str";
            bool isSourcePointer = sourceType.EndsWith("*") || sourceType == "str";

            if ((isTargetPointer && sourceType == "none*") ||
                (targetType == "none*" && isSourcePointer))
            {
                return true;
            }

            // Implicit promotion (e.g., f32 -> f64)
            if (targetType == "f64" && sourceType == "f32") return true;

            return false;
        }

        public static bool IsArrayType(string type) => type.Contains('[') && type.EndsWith("]");

        public static string GetArrayBaseType(string type)
        {
            if (!IsArrayType(type)) return type;
            return type[..type.IndexOf('[')];
        }

        public static int GetArraySize(string type)
        {
            if (!IsArrayType(type)) return 0;
            int start = type.IndexOf('[') + 1;
            string sizeStr = type.Substring(start, type.IndexOf(']') - start);
            return int.TryParse(sizeStr, out int size) ? size : 0;
        }
    }
}
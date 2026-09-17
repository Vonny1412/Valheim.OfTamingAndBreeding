using System;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Utilities
{
    internal class YamlUtils
    {

        public static string FormatException(YamlException ex, string yamlText, string context)
        {
            if (ex == null) return $"{context}: <no exception>";

            // 1) Get the innermost YamlException (it often has the best message/mark)
            YamlException root = ex;
            while (root.InnerException is YamlException inner)
                root = inner;

            var start = root.Start;

            int line = ClampToInt(start.Line);
            int column = ClampToInt(start.Column);

            var lines = (yamlText ?? string.Empty)
                .Replace("\r\n", "\n")
                .Split('\n');

            int lineIndex = line - 1;

            string errorLine = (lineIndex >= 0 && lineIndex < lines.Length)
                ? lines[lineIndex]
                : "<line unavailable>";

            int caretPos = column - 1;
            if (caretPos < 0) caretPos = 0;
            if (errorLine != null && caretPos > errorLine.Length)
                caretPos = errorLine.Length;

            string caret = new string(' ', caretPos) + "^";

            return $@"{context}
Error: {root.Message}
At line {line}, column {column}:

{errorLine}
{caret}";
        }

        private static int ClampToInt(long value)
        {
            if (value > int.MaxValue) return int.MaxValue;
            if (value < int.MinValue) return int.MinValue;
            return (int)value;
        }



    }
}

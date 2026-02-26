using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace OfTamingAndBreeding.Utils
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

            // Mark.Line/Column can be long -> robust clamping after int
            int line = ClampToInt(start.Line);
            int column = ClampToInt(start.Column);

            // 2) Text normalisieren & Zeilen splitten
            var lines = (yamlText ?? string.Empty).Replace("\r\n", "\n").Split('\n');

            string errorLine = (line >= 0 && line < lines.Length)
                ? lines[line]
                : "<line unavailable>";

            // Caret position clamps (not extending beyond line length)
            int caretPos = column;
            if (caretPos < 0) caretPos = 0;
            if (errorLine != null && caretPos > errorLine.Length) caretPos = errorLine.Length;

            string caret = new string(' ', caretPos) + "^";

            return $@"{context}
Error: {root.Message}
At line {line + 1}, column {caretPos + 1}:

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

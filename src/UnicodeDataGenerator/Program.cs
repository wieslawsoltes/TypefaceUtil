using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnicodeDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            // http://www.unicode.org/Public/UNIDATA/UnicodeData.txt
            // https://www.unicode.org/reports/tr44/#UnicodeData.txt
            var unicodeData = File.ReadAllLines("UnicodeData.txt", Encoding.UTF8);
            var charCodeNameMap = new Dictionary<int, string>(65536);

            for (int i = 0; i < unicodeData.Length; i++)
            {
                var fields = unicodeData[i].Split(';');
                var charCode = int.Parse(fields[0], NumberStyles.HexNumber);
                var charName = fields[1];

                if (charCode >= 0 && charCode <= 0xFFFF)
                {
                    if (charName.EndsWith(", First>"))
                    {
                        charName = charName.Replace(", First", String.Empty);
                        fields = unicodeData[++i].Split(';');
                        int endCharCode = int.Parse(fields[0], NumberStyles.HexNumber);

                        if (!fields[1].EndsWith(", Last>"))
                        {
                            throw new Exception("Expected end-of-range indicator.");
                        }

                        for (int charCodeInRange = charCode; charCodeInRange <= endCharCode; charCodeInRange++)
                        {
                            charCodeNameMap.Add(charCodeInRange, charName);
                        }
                    }
                    else
                    {
                        charCodeNameMap.Add(charCode, charName);
                    }
                }
            }

            var sb = new StringBuilder();

            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("");
            sb.AppendLine("namespace TypefaceUtil.OpenType");
            sb.AppendLine("{");
            sb.AppendLine("    public static class UnicodeData");
            sb.AppendLine("    {");
            sb.AppendLine("        public static Dictionary<int, string> CharCodeNameMap = new Dictionary<int, string>");
            sb.AppendLine("        {");

            foreach (var kvp in charCodeNameMap)
            {
                sb.AppendLine($"            [0x{kvp.Key:X2}] = \"{kvp.Value}\",");
            }
            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(@"..\..\..\..\src\TypefaceUtil.OpenType\UnicodeData.cs", sb.ToString());
        }
    }
}

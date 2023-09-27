using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public struct ReadWriteSetConfigLine : ConfigLine
    {
        public string[] ReadSet;
        public Dictionary<string, int> WriteDict;
    }

    internal class ReadWriteSetParser : Parser
    {
        private Regex regex = new Regex(@"^T\s*\(\s*(.*?)\s*\)\s*\(\s*(.*?)\s*\) *$", RegexOptions.Compiled);
        private Regex readSetRegex = new Regex(@"""(.*?)""", RegexOptions.Compiled);
        private Regex writeSetRegex = new Regex(@"<""([\w-]+)"",(\d+)>", RegexOptions.Compiled);
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = this.regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.ReadWriteSet,
                new ReadWriteSetConfigLine
                {
                    ReadSet = readSetRegex.Matches(match.Groups[1].Value).Select(m => m.Groups[1].Value).ToArray(),
                    WriteDict = writeSetRegex.Matches(match.Groups[2].Value)
                    .Select(s => writeSetRegex.Match(s.Value))
                    .ToDictionary(m => m.Groups[1].Value,m => int.Parse(m.Groups[2].Value))
                }
            );
        }
    }
}

using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers.ClientCommand
{
    public struct ReadWriteSetConfigLine : ClientCommandConfigLine
    {
        public ClientCommandConfigType Type => ClientCommandConfigType.ReadWriteSet;
        public List<string> ReadSet;
        public List<DadInt> WriteSet;
    }

    internal class ReadWriteSetParser : Parser
    {
        private Regex regex = new Regex(@"^T\s*\(\s*(.*?)\s*\)\s*\(\s*(.*?)\s*\) *$", RegexOptions.Compiled);
        private Regex readSetRegex = new Regex(@"""(.*?)""", RegexOptions.Compiled);
        private Regex writeSetRegex = new Regex(@"<""([\w-]+)"",(\d+)>", RegexOptions.Compiled);
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.ClientCommand,
                new ReadWriteSetConfigLine
                {
                    ReadSet = readSetRegex.Matches(match.Groups[1].Value)
                    .Select(m => m.Groups[1].Value)
                    .ToList(),
                    WriteSet = writeSetRegex.Matches(match.Groups[2].Value)
                    .Select(s => writeSetRegex.Match(s.Value))
                    .Select(m => new DadInt(m.Groups[1].Value, int.Parse(m.Groups[2].Value)))
                    .ToList()
                }
            );
        }
    }
}

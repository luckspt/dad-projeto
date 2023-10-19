using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers.ClientCommand
{
    public struct WaitConfigLine : ClientCommandConfigLine
    {
        public ClientCommandConfigType Type => ClientCommandConfigType.ReadWriteSet;
        public int Time;
    }

    internal class WaitParser : Parser
    {
        private Regex regex = new Regex(@"^W (\d+) *$", RegexOptions.Compiled);
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.ClientCommand,
                new WaitConfigLine
                {
                    Time = int.Parse(match.Groups[1].Value)
                }
            );
        }
    }
}

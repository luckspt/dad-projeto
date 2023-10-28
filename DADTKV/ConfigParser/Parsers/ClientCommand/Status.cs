using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers.ClientCommand
{
    public struct StatusConfigLine : ClientCommandConfigLine
    {
        public ClientCommandConfigType Type => ClientCommandConfigType.Status;
    }

    internal class StatusParser : Parser
    {
        private Regex regex = new Regex(@"^S *$", RegexOptions.Compiled);
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.ClientCommand,
                new StatusConfigLine()
            );
        }
    }
}

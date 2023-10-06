using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public struct SlotDurationConfigLine : ConfigLine
    {
        public int Duration;
    }

    internal class SlotDurationParser : Parser
    {
        private Regex regex = new Regex(@"^D (\d+) *$", RegexOptions.Compiled);
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = this.regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.SlotDuration,
                new SlotDurationConfigLine
                {
                    Duration = int.Parse(match.Groups[1].Value),
                }
            );
        }
    }
}

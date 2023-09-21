using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public struct TimeSlotConfigLine : ConfigLine
    {
        public int Time;
    }

    internal class TimeSlotParser : Parser
    {
        private Regex regex = new Regex(@"^D (\d+) *$");
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = this.regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.TimeSlot,
                new TimeConfigLine
                {
                    Time = int.Parse(match.Groups[1].Value),
                }
            );
        }
    }
}

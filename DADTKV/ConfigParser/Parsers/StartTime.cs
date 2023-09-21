using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public struct StartTimeConfigLine : ConfigLine
    {
        public string Time;
    }

    internal class StartTimeParser : Parser
    {
        private Regex regex = new Regex(@"^T (\d{1,2}:\d{1,2}:\d{1,2}) *$");
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = this.regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.StartTime,
                new StartTimeConfigLine
                {
                    Time = match.Groups[1].Value,
                }
            );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public enum ServerState
    {
        TransactionManager,
        LeaseManager
    }

    public struct ServerStateConfigLine : ConfigLine
    {
        public int TimeSlot;
        public string[] Processes;
        public Tuple<string, string>[] Suspected;
    }

    internal class ServerStateParser : Parser
    {
        private Regex regex = new Regex(@"^F (\d+) (([NC] )+)((\(\w+,\w+\) *)+) *$", RegexOptions.Compiled);
        private Regex suspectedRegex = new Regex(@"(\w+),(\w+)", RegexOptions.Compiled);
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = this.regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.ServerState,
                new ServerStateConfigLine
                {
                    TimeSlot = int.Parse(match.Groups[1].Value),
                    Processes = match.Groups[2].Value.Trim().Split(" "),
                    Suspected = match.Groups[4].Value.Trim().Split(" ").Select(s =>
                    {
                        Match suspectedMatch = this.suspectedRegex.Match(s);
                        return new Tuple<string, string>(
                                                       suspectedMatch.Groups[1].Value,
                                                                                  suspectedMatch.Groups[2].Value
                                                                                                         );
                    }).ToArray()
                }
            );
        }
    }
}

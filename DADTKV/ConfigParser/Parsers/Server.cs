using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public enum ServerType
    {
        TransactionManager,
        LeaseManager
    }

    public struct ServerConfigLine : ConfigLine
    {
        public string ID;
        public ServerType Type;
        public string Url;
    }

    internal class ServerParser : Parser
    {
        private Regex regex = new Regex(@"^P (\w+) (T|L) (http:\/\/(\d{1,3}.){3}\d{1,3}:\d{1,5}) *$", RegexOptions.Compiled);
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = this.regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.Server,
                new ServerConfigLine
                {
                    ID = match.Groups[1].Value,
                    Type = match.Groups[2].Value == "T" ? ServerType.TransactionManager : ServerType.LeaseManager,
                    Url = match.Groups[3].Value
                }
            );
        }
    }
}

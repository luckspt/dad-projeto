using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Parsers.ClientCommand
{
    public interface ClientCommandConfigLine : ConfigLine
    {
        public ClientCommandConfigType Type { get; }
    }

    public enum ClientCommandConfigType
    {
        // Wait (W)
        Wait,
        // Transaction (T)
        ReadWriteSet,
        // Status (S)
        Status,
    }
}

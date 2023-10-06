using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    public struct PromiseResponse
    {
        public int Slot;
        public int WriteEpoch;
        public Dictionary<string, List<string>>? Leases;
        public Dictionary<string, List<string>>? SelfLeases;
    }

    public struct Promises
    {
        public int GreatestWriteEpoch;
        public int ReceivedCount;
    }
}

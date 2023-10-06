using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    public struct AcceptedResponse
    {
        public int Slot;
        public int Epoch;
        public Dictionary<string, List<string>>? Leases;
    }

    public struct Accepteds
    {
        /// <summary>
        /// Key is the proposal number, value is the number of accepteds received for that proposal number.
        /// </summary>
        public Dictionary<int, int> ReceivedCount;
    }
}

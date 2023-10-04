using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    public class Phase2
    {
        private PaxosInstance paxos;
        private int acceptedCount;

        public Phase2(PaxosInstance paxos)
        {
            this.paxos = paxos;
            this.acceptedCount = 0;
        }

        public bool Accept(int epoch, Dictionary<string, List<string>> leases)
        {
            // We only accept if the epoch is the same as the one we have promised
            if (this.paxos.ReadTimestamp != epoch)
            {
                return false;
            }

            this.paxos.Value = leases;
            this.paxos.WriteTimestamp = epoch;
            this.acceptedCount = 0;

            // TODO: send accepted(epoch, paxos.Value or leases) back to proposer

            return true;
        }

        public bool Accepted(int epoch, Dictionary<string, List<string>> leases)
        {
            this.acceptedCount++;

            // We only care about the majority, so we can ignore the rest
            if (this.acceptedCount >= this.paxos.peers.Count / 2)
            {
                // We have consensus!!!
                return false;
            }

            return true;
        }
    }
}

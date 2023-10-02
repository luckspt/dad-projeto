using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    struct Promises
    {
        public int greatestWriteEpoch;
        public int receivedCount;
    }

    internal class Phase1
    {
        private PaxosInstance paxos;
        private Promises promises;

        public Phase1(PaxosInstance paxos)
        {
            this.paxos = paxos;
            this.promises = new Promises { greatestWriteEpoch = 0, receivedCount = 0 };
        }

        public bool Prepare(int epoch)
        {
            // The acceptors only promise when the epoch is higher than the highest epoch they have seen
            if (this.paxos.ReadTimestamp >= epoch)
            {
                return false;
            }

            this.paxos.ReadTimestamp = epoch;

            // TODO: send promise(epoch, paxos.Value) back to proposer

            return true;
        }

        public bool Promise(int epoch, Dictionary<string, List<string>> leases)
        {
            this.promises.receivedCount++;

            // We only care about the majority, so we can ignore the rest
            if (this.promises.receivedCount >= this.paxos.peers.Count / 2)
            {
                // TODO: somewhere else (or here? :thinking:). If here, be careful with the >=
                // 1. check if epoch=0.
                // 1.a: If so, we propose our own value.
                // 1.b: Otherwise, use the highest value we have seen.
                // 2. send accept(epoch, value) to all acceptors
                return false;
            }

            // If the epoch is higher than the highest epoch we have seen, update our values
            if (epoch > this.promises.greatestWriteEpoch)
            {
                this.promises.greatestWriteEpoch = epoch;
                this.paxos.Value = leases;
            }

            return true;
        }
    }
}

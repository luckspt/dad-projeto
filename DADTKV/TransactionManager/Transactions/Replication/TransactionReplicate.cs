using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Transactions.Replication
{
    internal class TransactionReplicate
    {
        private List<URBMessage> pending;
        private List<URBMessage> delivered;
        private Dictionary<URBMessage, HashSet<Peer>> acks;
        private HashSet<Peer> correct;

        public void AddToPending(URBMessage message)
        {
            this.pending.Add(message);
        }

        public bool IsPending(URBMessage message)
        {
            return this.pending.Contains(message);
        }

        public void AddToAcks(URBMessage message, Peer peer)
        {
            if (!this.acks.Contains(message))
                this.acks.Add(message);

            if (this.acks[message] == null)
                this.acks[message] = new List<Peer> { peer };
            else
                this.acks[message].Add(peer);
        }

        public bool CanDeliver(URBMessage message)
        {
            // Majority
            int howMany = this.correct.Where(p => this.acks[message].Contains(p)).Count();
            return howMany > this.correct.Count() / 2;
        }

        public bool RemoveCorrect(Peer crashed)
        {
            this.correct.Remove(crashed);
        }
    }
}

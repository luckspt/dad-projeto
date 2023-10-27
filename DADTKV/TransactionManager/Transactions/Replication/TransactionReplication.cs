using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManager.Transactions.Replication.Client;

namespace TransactionManager.Transactions.Replication
{
    // Implemented with Majority-Ack Uniform Reliable Broadcast
    internal class TransactionReplication
    {
        public TransactionReplicationServiceClient ServiceClient { get; }
        public HashSet<Peer> Correct { get; }
        private Dictionary<ReplicationMessage, HashSet<string>> pending;
        private HashSet<ReplicationMessage> delivered;
        private Dictionary<ReplicationMessage, HashSet<string>> acks;
        private Action<ReplicationMessage> callOnDeliver;

        public TransactionReplication(HashSet<Peer> correct, Action<ReplicationMessage> callOnDeliver)
        {
            this.ServiceClient = new TransactionReplicationServiceClient(this);
            this.Correct = correct;
            this.pending = new Dictionary<ReplicationMessage, HashSet<string>>();
            this.delivered = new HashSet<ReplicationMessage>();
            this.acks = new Dictionary<ReplicationMessage, HashSet<string>>();
            this.callOnDeliver = callOnDeliver;
        }

        public void AddToPending(ReplicationMessage message, string sender)
        {
            if (!this.pending.ContainsKey(message))
                this.pending.Add(message, new HashSet<string>() { sender });
            else
                this.pending[message].Add(sender);
        }

        public bool IsPending(ReplicationMessage message, string sender)
        {
            return this.pending.ContainsKey(message)
                && this.pending[message].Contains(sender);
        }

        public void AddToAcks(ReplicationMessage message, string sender)
        {
            if (!this.acks.ContainsKey(message))
                this.acks.Add(message, new HashSet<string> { sender });
            else if (this.acks[message] == null)
                this.acks[message] = new HashSet<string> { sender };
            else
                this.acks[message].Add(sender);
        }

        public bool CanDeliver(ReplicationMessage message)
        {
            // Majority-Acks URB
            int howMany = this.Correct.Where(p => this.acks[message].Contains(p.Id)).Count();
            return howMany > (this.Correct.Count() / 2);
        }

        public bool HasBeenDelivered(ReplicationMessage message)
        {
            return this.delivered.Contains(message);
        }

        public void Deliver(ReplicationMessage message)
        {
            this.delivered.Add(message);
            this.callOnDeliver(message);
        }

        public bool RemoveCorrect(Peer crashed)
        {
            return this.Correct.Remove(crashed);
        }
    }
}

using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManager.Transactions.Replication.Client;

namespace TransactionManager.Transactions.Replication.Server
{
    internal class TransactionReplicationServiceLogic
    {
        private TransactionReplication transactionReplicate;
        public TransactionReplicationServiceLogic(TransactionReplication transactionReplicate, TransactionReplicationServiceClient transactionReplicationServiceClient)
        {
            this.transactionReplicate = transactionReplicate;
        }

        public bool URBBroadcast(BroadcastMessage message, string sender)
        {
            lock (this.transactionReplicate)
            {
                this.transactionReplicate.AddToPending(message, sender);
                this.transactionReplicate.ServiceClient.BEBroadcast(message);

                return true;
            }
        }

        public bool BEBDeliver(BroadcastMessage message, string sender)
        {
            lock (this.transactionReplicate)
            {
                this.transactionReplicate.AddToAcks(message, sender);

                if (this.transactionReplicate.IsPending(message, sender))
                {
                    if (!this.transactionReplicate.HasBeenDelivered(message) && this.transactionReplicate.CanDeliver(message))
                        this.transactionReplicate.Deliver(message);
                }
                else
                {
                    this.transactionReplicate.AddToPending(message, sender);
                    this.transactionReplicate.ServiceClient.BEBroadcast(message);
                }

                return true;
            }
        }

        // TODO implement the suspect processes here
        public bool OnCrashDetected(Peer crashed)
        {
            lock (this.transactionReplicate)
            {
                return this.transactionReplicate.RemoveCorrect(crashed);
            }
        }
    }
}

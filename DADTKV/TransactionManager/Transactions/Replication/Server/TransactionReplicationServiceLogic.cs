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
        private TransactionReplicate transactionReplicate;
        private TransactionReplicationServiceClient transactionReplicationServiceClient;

        private void OnURBBroadcast(URBMessage message)
        {
            this.transactionReplicate.AddToPending(message);
            this.transactionReplicationServiceClient.TriggerBEBBroadcast(message);
        }

        private void OnBEBDeliver(URBMessage message, Peer sender)
        {
            this.transactionReplicate.AddToAcks(message, sender);
            if (!this.transactionReplicate.IsPending(message))
            {
                this.transactionReplicate.AddToPending(message);
                this.transactionReplicationServiceClient.TriggerBEBBroadcast(message);
            }
        }

        private void OnCrashDetected(Peer crashed)
        {
            this.transactionReplicate.RemoveCorrect(crashed);
        }
    }
}

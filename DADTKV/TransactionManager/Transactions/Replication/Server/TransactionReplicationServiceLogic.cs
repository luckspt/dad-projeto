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
        private TransactionManager transactionManager;
        public TransactionReplicationServiceLogic(TransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public bool URBBroadcast(ReplicationMessage message, string senderId)
        {
            lock (this.transactionManager.TransactionReplication)
            {
                Logger.GetInstance().Log($"TransactionReplicationService.OnURB", $"Adding message to pending and BEB Broadcasting");

                this.transactionManager.TransactionReplication.AddToPending(message, senderId);
                this.transactionManager.TransactionReplication.ServiceClient.BEBroadcast(message, this.transactionManager.ManagerId);

                return true;
            }
        }

        public bool BEBDeliver(ReplicationMessage message, string senderId)
        {
            Logger.GetInstance().Log($"TransactionReplicationService.OnBEB", $"Waiting for sender to own all leases");

            // !!!!IMPORTANT!!! CHECK IF THE TRANSACTION EXECUTOR HOLDS ALL THE LEASES LOCALLY!!!!
            Transaction transaction = Transaction.FromReplicationMessage(message);
            transaction.WaitToExecute(this.transactionManager, true);
            // After this, it is assured that the transaction executer holds the leases

            lock (this.transactionManager.TransactionReplication)
            {
                Logger.GetInstance().Log($"TransactionReplicationService.OnBEB", $"Acknowledging message");
                this.transactionManager.TransactionReplication.AddToAcks(message, senderId);

                if (this.transactionManager.TransactionReplication.IsPending(message, senderId))
                {
                    bool canDeliver = !this.transactionManager.TransactionReplication.HasBeenDelivered(message) && this.transactionManager.TransactionReplication.CanDeliver(message);
                    Logger.GetInstance().Log($"TransactionReplicationService.OnBEB", $"Message was pending and hasBeenDelivered={this.transactionManager.TransactionReplication.HasBeenDelivered(message)}, canDeliver={this.transactionManager.TransactionReplication.CanDeliver(message)}, outcome={canDeliver}");

                    if (canDeliver)
                        this.transactionManager.TransactionReplication.Deliver(message);
                }
                else // NOT Pending
                {
                    Logger.GetInstance().Log($"TransactionReplicationService.OnBEB", $"Message was NOT pending. Adding to pending and BEB Broadcasting");
                    this.transactionManager.TransactionReplication.AddToPending(message, senderId);
                    this.transactionManager.TransactionReplication.ServiceClient.BEBroadcast(message, this.transactionManager.ManagerId);
                }

                return true;
            }
        }

        // TODO implement the suspect processes here
        public bool OnCrashDetected(Peer crashed)
        {
            lock (this.transactionManager.TransactionReplication)
            {
                return this.transactionManager.TransactionReplication.RemoveCorrect(crashed);
            }
        }
    }
}

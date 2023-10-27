using Common;
using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TransactionManager.Leases;
using TransactionManager.Store;
using TransactionManager.Transactions;
using TransactionManager.Transactions.Replication;

namespace TransactionManager
{
    class TransactionReplyLock
    {
        public List<DadInt> ReplyValue;
        public bool CanReply;
    }

    internal class TransactionManager
    {
        // The id of this transaction manager
        public String ManagerId { get; }
        // Leasing service
        public Leasing Leasing { get; private set; }
        public List<Peer> TransactionManagers { get; private set; }
        public KVStore KVStore { get; private set; }
        public TransactionRequestsBuffer TransactionsBuffer { get; private set; }
        public Dictionary<string, KeyValuePair<TransactionReplyLock, Transactions.Transaction>> TransactionReplyLocks { get; }
        public TransactionReplication TransactionReplication { get; private set; }


        public TransactionManager(String managerId)
        {
            this.ManagerId = managerId;
            this.KVStore = new KVStore();
            this.TransactionsBuffer = new TransactionRequestsBuffer();
            this.TransactionReplyLocks = new Dictionary<string, KeyValuePair<TransactionReplyLock, Transactions.Transaction>>();
        }

        public void Start(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses)
        {
            this.Leasing = new Leasing(this, leaseManagersAddresses.Select(address => Peer.FromString(address)).ToList());
            this.TransactionManagers = transactionManagersAddresses.Select(address => Peer.FromString(address)).ToList();
            this.TransactionReplication = new TransactionReplication(this.TransactionManagers.ToHashSet(), this.OnTransactionReplicated);

            // Thread to run the transactions (sequentially)
            Task.Run(() =>
            {
                while (true)
                    this.HandleTransactionRequests();
            });
        }

        private void HandleTransactionRequests()
        {
            Transactions.Transaction transaction;
            lock (this.TransactionsBuffer)
            {
                while (this.TransactionsBuffer.Count == 0)
                {
                    Logger.GetInstance().Log($"TransactionManager.HandleTransactionRequests", $"Waiting for requests");
                    Monitor.Wait(this.TransactionsBuffer);
                }

                // Get the latest transaction in the Queue
                transaction = this.TransactionsBuffer.Take();
            }

            Logger.GetInstance().Log($"TransactionManager.HandleTransactionRequests", $"Working on a transaction (waiting for leases)");

            // Wait until we can operate on it (we own all the necessary leases)
            Program.ManagerClient.Status = "WaitingLease";
            transaction.WaitToExecute(this);

            KeyValuePair<TransactionReplyLock, Transactions.Transaction> replyLock;
            lock (this.TransactionReplyLocks)
            {
                replyLock = this.TransactionReplyLocks[transaction.Guid];
            }

            // Execute the reads and then the writes
            lock (replyLock.Key)
            {
                Program.ManagerClient.Status = "ExecutingTransaction";
                Logger.GetInstance().Log($"TransactionManager.HandleTransactionRequests", $"Working on a transaction (has leases)");

                // Read
                List<DadInt> reads = transaction.ExecuteReads(this.KVStore);

                // Populate the ReplyValue and Pulse it
                replyLock.Key.ReplyValue = reads;
                Monitor.PulseAll(replyLock.Key);
                Logger.GetInstance().Log($"TransactionManager.HandleTransactionRequests", $"Reads executed, Pulsing all replyLock waiting threads");

                // Write (async messages)
                transaction.ExecuteWrites(this);
            }
        }


        // Called when a transaction commits (on each replica)
        private void OnTransactionReplicated(Transactions.Replication.ReplicationMessage message)
        {
            lock (this)
            {
                lock (this.KVStore)
                {
                    Logger.GetInstance().Log($"TransactionManager.OnTransactionReplicated", $"Applying writes locally");
                    // Apply writes locally
                    foreach (Transactions.Replication.RPCStoreDadInt receivedDadInt in message.DadInts)
                    {
                        StoreDadInt storeDadInt;
                        if (!this.KVStore.TryGetValue(receivedDadInt.Key, out storeDadInt))
                        {
                            storeDadInt = new StoreDadInt();
                            this.KVStore.Add(receivedDadInt.Key, storeDadInt);
                        }

                        // Update if
                        // - Older epoch (maybe from other TM but we don't distinguish)
                        // - Same epoch but new version (could be from releasing lease in TM1 and TM2 commiting a transaction)
                        // last write wins!
                        if (receivedDadInt.Epoch > storeDadInt.LastWriteEpoch || (receivedDadInt.Epoch == storeDadInt.LastWriteEpoch
                                && receivedDadInt.EpochWriteVersion > storeDadInt.EpochWriteVersion))
                        {
                            storeDadInt.LastWriteEpoch = receivedDadInt.Epoch;
                            storeDadInt.EpochWriteVersion = receivedDadInt.EpochWriteVersion;
                            storeDadInt.Value = receivedDadInt.Value;
                        }
                    }
                }

                // Allow reply to the client and Pulse it
                try
                {
                    KeyValuePair<TransactionReplyLock, Transactions.Transaction> replyLock = this.TransactionReplyLocks[message.Guid];

                    Logger.GetInstance().Log($"TransactionManager.OnTransactionReplicated", $"Will reply to client");

                    lock (replyLock.Key)
                    {
                        Logger.GetInstance().Log($"TransactionManager.OnTransactionReplicated", $"Pulsing other thread");
                        replyLock.Key.CanReply = true;
                        Monitor.PulseAll(replyLock.Key);
                    }

                    Logger.GetInstance().Log($"TransactionManager.OnTransactionReplicated", $"Cleanup leases");
                }
                catch
                {
                    // Does not have the replyLock
                    Logger.GetInstance().Log("OnTransactionReplicated", "Will NOT reply to client (does not have replyLock)");
                }
            };

            // Check and release if the transaction was holding conflicting leases
            lock (this.Leasing)
            {
                List<string> leasesToCheck = message.ReadDadInts.Concat(message.DadInts.Select(x => x.Key)).ToList();
                List<string> leasesToFree = leasesToCheck.Where(x => this.Leasing.HasLease(x, message.ExecutingManagerId)).ToList();
                Logger.GetInstance().Log("OnTransactionReplicated", $"Will free checked={string.Join(",", leasesToCheck)} toFree={string.Join(",", leasesToFree)}");
                this.Leasing.Free(leasesToFree, message.ExecutingManagerId);
            }
        }
    }
}

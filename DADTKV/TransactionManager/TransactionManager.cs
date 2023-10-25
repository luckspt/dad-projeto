using Common;
using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManager.Leases;
using TransactionManager.Store;
using TransactionManager.Transactions;

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
        private String managerId;
        // Leasing service
        public Leasing Leasing { get; private set; }
        public List<Peer> TransactionManagers { get; private set; }
        public KVStore KVStore { get; private set; }
        public TransactionRequestsBuffer TransactionsBuffer { get; private set; }
        public Dictionary<Transaction, TransactionReplyLock> TransactionReplyLocks { get; } = new Dictionary<Transaction, TransactionReplyLock>();


        public TransactionManager(String managerId)
        {
            this.managerId = managerId;
            this.KVStore = new KVStore();
            this.TransactionsBuffer = new TransactionRequestsBuffer();
        }

        public void Start(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses)
        {
            this.Leasing = new Leasing(this.managerId, leaseManagersAddresses.Select(address => new Peer(address)).ToList());
            this.TransactionManagers = transactionManagersAddresses.Select(address => new Peer(address)).ToList();

            // Thread to run the transactions (sequentially)
            Task.Run(() =>
            {
                while (true)
                    this.HandleTransactionRequests();
            });
        }

        private void HandleTransactionRequests()
        {
            // TODO can be improved by using monitors
            if (this.TransactionsBuffer.Count != 0)
            {
                // Get the latest transaction in the Queue
                Transaction transaction = this.TransactionsBuffer.Take();

                // Wait until we can operate on it (we own all the necessary leases)
                transaction.WaitToExecute(this.Leasing);

                // Execute the reads and then the writes
                TransactionReplyLock replyLock = this.TransactionReplyLocks[transaction];
                lock (replyLock)
                {
                    // Read
                    List<DadInt> reads = transaction.ExecuteReads(this.Leasing, this.KVStore);

                    // Populate the ReplyValue and Pulse the replyLock
                    replyLock.ReplyValue = reads;
                    Monitor.Pulse(replyLock);

                    // Write
                    transaction.ExecuteWrites(this.managerId, this.Leasing, this.KVStore);
                }
            }
        }
    }
}

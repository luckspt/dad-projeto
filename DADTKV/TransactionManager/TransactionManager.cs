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
    public class TMPeer
    {
        public string Address { get; }

        public TMPeer(string address)
        {
            this.Address = address;
        }
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
                Transaction transaction = this.TransactionsBuffer.Take();

                List<string> keysToReadKeys = transaction.ReadOperations.Select(t => t.Key).ToList();
                List<string> keysToWriteKeys = transaction.WriteOperations.Select(t => t.Key).ToList();

                Dictionary<string, bool> allKeys = keysToReadKeys.Concat(keysToWriteKeys).ToDictionary(key => key, key => this.Leasing.HasLease(key));
                bool hasAllLeases = false;

                while (!hasAllLeases)
                {
                    hasAllLeases = allKeys.All(x => x.Value == true);
                    // TODO Monitors on Lease update (and lease freeing to make this method advance)
                }

                // Now we have all the leases
                // Execute the reads --
                List<KeyValuePair<string, StoreDadInt>> keysToRead = this.KVStore.Where(x => keysToReadKeys.Contains(x.Key)).ToList();

                // return keysRead to client
                List<DadInt> keysRead = keysToRead.Select(x => new DadInt(x.Key, x.Value.Value)).ToList();
                // --

                // Execute the writes --
                // TODO replicate
                // --

                // Check if there is any lease that's conflicting after executing this transaction --
                List<string> conflictingLeases = allKeys
                        .Where(x => this.Leasing.IsConflicting(x.Key))
                        .Select(x => x.Key)
                        .ToList();

                // Free them
                if (conflictingLeases.Count > 0)
                    this.Leasing.Free(conflictingLeases);
                // --
            }
        }
    }
}

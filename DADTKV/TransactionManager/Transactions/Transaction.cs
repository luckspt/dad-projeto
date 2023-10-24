using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TransactionManager.Leases;
using TransactionManager.Store;

namespace TransactionManager.Transactions
{
    internal class Transaction
    {
        public string ClientId { get; }
        public List<ReadOperation> ReadOperations { get; }
        public List<WriteOperation> WriteOperations { get; }

        public Transaction(string clientId, List<ReadOperation> readOperations, List<WriteOperation> writeOperations)
        {
            this.ClientId = clientId;
            this.ReadOperations = readOperations;
            this.WriteOperations = writeOperations;
        }

        // Block until we have all the leases
        public void WaitToExecute(Leasing leasing)
        {
            List<string> keysToReadKeys = this.ReadOperations.Select(t => t.Key).ToList();
            List<string> keysToWriteKeys = this.WriteOperations.Select(t => t.Key).ToList();

            Dictionary<string, bool> allKeys = keysToReadKeys.Concat(keysToWriteKeys).ToDictionary(key => key, key => leasing.HasLease(key));
            bool hasAllLeases = false;

            while (!hasAllLeases)
            {
                hasAllLeases = allKeys.All(x => x.Value == true);
                // TODO Monitors on Lease update (and lease freeing to make this method advance)
            }
        }

        public List<DadInt> ExecuteReads(Leasing leasing, KVStore kvStore)
        {
            List<string> keysToReadKeys = this.ReadOperations.Select(t => t.Key).ToList();
            List<KeyValuePair<string, StoreDadInt>> keysToRead = kvStore.Where(x => keysToReadKeys.Contains(x.Key)).ToList();

            return keysToRead.Select(x => new DadInt(x.Key, x.Value.Value)).ToList();
        }

        // Block until writes are propagated
        public void ExecuteWrites(string managerId, Leasing leasing, KVStore kvStore)
        {
            // TODO URB.Broadcast()
            // Monitor or something
        }

        public void CleanupLeases(Leasing leasing)
        {
            List<string> keysToReadKeys = this.ReadOperations.Select(t => t.Key).ToList();
            List<string> keysToWriteKeys = this.WriteOperations.Select(t => t.Key).ToList();

            List<string> allKeys = keysToReadKeys.Concat(keysToWriteKeys).ToList();

            List<string> conflictingLeases = allKeys
                    .Where(x => leasing.IsConflicting(x))
                    .ToList();

            // Free them
            if (conflictingLeases.Count > 0)
                leasing.Free(conflictingLeases);
        }
    }

    internal class ReadOperation
    {
        public string Key { get; }

        public ReadOperation(string key)
        {
            this.Key = key;
        }
    }

    internal class WriteOperation
    {
        public string Key { get; }
        public DadIntValue Value { get; }

        public WriteOperation(string key, DadIntValue value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}

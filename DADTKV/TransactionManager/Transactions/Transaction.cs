using Common;
using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TransactionManager;
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
            Program.ManagerClient.Status = "WaitingLease";

            // Lock leasing because we want to Pulse/Wait for it
            lock (leasing)
            {
                List<string> keysToReadKeys = this.ReadOperations.Select(t => t.Key).ToList();
                List<string> keysToWriteKeys = this.WriteOperations.Select(t => t.Key).ToList();

                // Check if I need to request more leases
                List<string> keysINeed = keysToReadKeys.Concat(keysToWriteKeys).Where(key => !leasing.HasLease(key)).ToList();
                if (keysINeed.Count > 0)
                {
                    Logger.GetInstance().Log("Transaction.WaitToExecute", $"I don't have {string.Join(",", keysINeed)}, so I'm requesting");
                    leasing.Request(keysINeed);
                }

                // Wait until we own all the leases
                while (!keysINeed.All(x => leasing.HasLease(x)))
                {
                    Logger.GetInstance().Log("Transaction.WaitToExecute", $"Waiting for the leases...");
                    Monitor.Wait(leasing);
                }
            }
        }

        public List<DadInt> ExecuteReads(Leasing leasing, KVStore kvStore)
        {
            Program.ManagerClient.Status = "ExecutingTransaction";

            lock (kvStore)
            {
                List<string> keysToReadKeys = this.ReadOperations.Select(t => t.Key).ToList();
                List<KeyValuePair<string, StoreDadInt>> keysToRead = kvStore.Where(x => keysToReadKeys.Contains(x.Key)).ToList();

                return keysToRead.Select(x => new DadInt(x.Key, x.Value.Value)).ToList();
            }
        }

        // Block until writes are propagated
        public void ExecuteWrites(TransactionManager transactionManager)
        {
            Program.ManagerClient.Status = "ExecutingTransaction";

            Replication.BroadcastMessage message = new Replication.BroadcastMessage
            {
                OriginReplyLockHash = transactionManager.TransactionReplyLocks[this].GetHashCode(),
                DadInts = this.DadIntsToWrite(transactionManager.KVStore, transactionManager.Leasing.Epoch)
            };

            transactionManager.TransactionReplication.ServiceClient.URBroadcast(message, transactionManager.ManagerId);
        }

        private List<Replication.RPCStoreDadInt> DadIntsToWrite(KVStore keyValues, int currentEpoch)
        {
            List<Replication.RPCStoreDadInt> dataToWrite = this.WriteOperations.Select(w =>
            {
                StoreDadInt storeDadInt;
                if (!keyValues.TryGetValue(w.Key, out storeDadInt) || storeDadInt.LastWriteEpoch < currentEpoch)
                {
                    storeDadInt = new StoreDadInt()
                    {
                        LastWriteEpoch = currentEpoch,
                        EpochWriteVersion = 0,
                    };
                }

                return new Replication.RPCStoreDadInt
                {
                    Key = w.Key,
                    Value = w.Value,
                    Epoch = storeDadInt.LastWriteEpoch,
                    EpochWriteVersion = storeDadInt.EpochWriteVersion + 1,
                };
            }).ToList();

            return dataToWrite;
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

        public override string ToString()
        {
            return $"{this.Key}={this.Value}";
        }
    }
}

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
        public string ExecutingManagerId { get; } // For some reason we receive this but never use it... Whatever..
        public string Guid { get; }
        public List<ReadOperation> ReadOperations { get; }
        public List<WriteOperation> WriteOperations { get; }

        public Transaction(string executingManagerId, string guid, List<ReadOperation> readOperations, List<WriteOperation> writeOperations)
        {
            this.ExecutingManagerId = executingManagerId;
            this.Guid = guid;
            this.ReadOperations = readOperations;
            this.WriteOperations = writeOperations;
        }

        public static Transaction FromReplicationMessage(Replication.ReplicationMessage message)
        {
            return new Transaction(message.ExecutingManagerId, message.Guid,
                message.ReadDadInts.Select(x => new ReadOperation(x)).ToList(),
                message.DadInts.Select(x => new WriteOperation(x.Key, x.Value)).ToList());
        }

        public List<string> GetLeasesKeys()
        {
            return this.ReadOperations.Select(t => t.Key)
                .Concat(this.WriteOperations.Select(t => t.Key))
                .ToList();
        }

        // Block until we have all the leases
        public void WaitToExecute(Leasing leasing, bool isRequestingDisabled = false)
        {
            // Lock leasing because we want to Pulse/Wait for it
            lock (leasing)
            {
                // Check if I need to request more leases
                List<string> keysINeed = this.GetLeasesKeys().Where(key => !leasing.HasLease(key, this.ExecutingManagerId)).ToList();
                if (keysINeed.Count > 0 && !isRequestingDisabled)
                {
                    Logger.GetInstance().Log("Transaction.WaitToExecute", $"Don't have {string.Join(",", keysINeed)}, so I'm waiting");
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

        public List<DadInt> ExecuteReads(KVStore kvStore)
        {
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
            Replication.ReplicationMessage message = new Replication.ReplicationMessage
            {
                Guid = this.Guid,
                ExecutingManagerId = this.ExecutingManagerId,
                DadInts = this.DadIntsToWrite(transactionManager.KVStore, transactionManager.Leasing.Epoch),
                ReadDadInts = this.ReadOperations.Select(x => x.Key).ToList(),
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

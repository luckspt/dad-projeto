using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

using Common;
using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Transactions
{
    internal class TransactionRequestsBuffer
    {
        public int Count { get => this.buffer.Count; }

        private List<Transaction> buffer;

        public TransactionRequestsBuffer()
        {
            buffer = new List<Transaction>();
        }

        public void Add(string clientId, List<ReadOperation> keysToRead, List<WriteOperation> keysToWrte)
        {
            lock (this)
            {
                Transaction transaction = new Transaction(
                    clientId,
                    keysToRead,
                    keysToWrte
                );

                buffer.Add(transaction);
            }
        }

        public Transaction Take()
        {
            lock (this)
            {
                Transaction transaction = buffer[0];
                buffer.RemoveAt(0);

                return transaction;
            }
        }
    }
}

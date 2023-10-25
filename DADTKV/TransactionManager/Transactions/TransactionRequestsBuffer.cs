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

        public void Add(Transaction transaction)
        {
            buffer.Add(transaction);
        }

        public Transaction Take()
        {
            Transaction transaction = buffer[0];
            buffer.RemoveAt(0);

            return transaction;
        }
    }
}

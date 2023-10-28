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

        public void AddToHead(Transaction transaction)
        {
            this.buffer.Insert(0, transaction);
        }

        public void Add(Transaction transaction)
        {
            this.buffer.Add(transaction);
        }

        public Transaction Get(int index)
        {
            return this.buffer[index];
        }

        public Transaction Take()
        {
            Transaction transaction = buffer[0];
            buffer.RemoveAt(0);

            return transaction;
        }
    }
}

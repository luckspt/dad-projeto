using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManager.Store;

namespace TransactionManager.Transactions
{
    internal class TransactionRunningServiceLogic
    {
        public TransactionManager transactionManager;
        public TransactionRunningServiceLogic(TransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public List<DadInt> RunTransaction(string clientId, List<ReadOperation> keysToRead, List<WriteOperation> keysToWrite)
        {
            // Add transaction to be executed
            this.transactionManager.TransactionsBuffer.Add(clientId, keysToRead, keysToWrite);

            // TODO: MONITOR or something to only run what's next ONLY if the writes are replicated

            // Return read values
            return new List<DadInt>();
        }
    }
}

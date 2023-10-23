using Common;
using TransactionManager.Leases.LeaseRequesting;

namespace DADTKV.Transactions
{
    public class Transactions
    {
        private TransactionsServiceClient client = new TransactionsServiceClient();
        private Peer transactionManager;

        public Transactions(Peer transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public List<DadInt> TxSubmit(string clientId, List<string> toRead, List<DadInt> toWrite)
        {
            try
            {
                return this.client.RunTransaction(this.transactionManager, toRead, toWrite);
            }
            catch
            {
                return new List<DadInt>() { DadInt.CreateAborted() };
            }
        }

        public bool Status()
        {
            return true;
        }
    }
}
using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Status
{
    internal class StatusServiceLogic
    {
        private TransactionManager transactionManager;
        public StatusServiceLogic(TransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public void Status()
        {
            string message = "\n\nSTATUS:";

            message += $"Transactions:\n- {this.transactionManager.TransactionsBuffer.Count} are buffered to execute";

            List<string> ownedLeases = this.transactionManager.Leasing.GetOwnedLeases();
            List<int> leaseUpdatesToApply = this.transactionManager.Leasing.LeaseReceptionBuffer.GetEpochsToApply();
            message += $"Leases:\n- We're on epoch {this.transactionManager.Leasing.Epoch}\n- I own {ownedLeases.Count} ({string.Join(",", ownedLeases)})\n- {leaseUpdatesToApply.Count} lease updates buffered to be applies ({string.Join(",", leaseUpdatesToApply)})";

            System.Console.WriteLine(message);
        }
    }
}

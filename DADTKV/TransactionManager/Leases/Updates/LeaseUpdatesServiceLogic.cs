using Common;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Leases.LeaseUpdates
{
    internal class LeaseUpdatesServiceLogic
    {
        private TransactionManager transactionManager;
        private Dictionary<int, int> receptionCounts = new Dictionary<int, int>();

        public LeaseUpdatesServiceLogic(TransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public bool LeaseUpdate(LeaseUpdateRequest update)
        {
            bool hasEpoch = false;

            lock (this.transactionManager.Leasing.LeaseReceptionBuffer)
            {
                hasEpoch = this.transactionManager.Leasing.LeaseReceptionBuffer.Has(update.Epoch);
            }

            Logger.GetInstance().Log("LeaseUpdateService", $"Received a Lease update for epoch={update.Epoch}!");

            // We already applied of have had a majority, so ignore
            if (this.transactionManager.Leasing.Epoch > update.Epoch || hasEpoch)
                return true;

            // Check if we have a majority or not
            lock (this.receptionCounts)
            {
                int count = 0;
                if (this.receptionCounts.ContainsKey(update.Epoch))
                    count = this.receptionCounts[update.Epoch];

                count++;

                lock (this.transactionManager.Leasing)
                {
                    int needed = (this.transactionManager.Leasing.LeaseManagers.Count - 1) / 2 + 1;
                    Logger.GetInstance().Log("LeaseUpdateService", $"Got {count}/{needed} updates for epoch={update.Epoch}");

                    // First arrival define the start of the new epoch
                    if (count == 1)
                        this.transactionManager.Leasing.Epoch = update.Epoch;

                    if (count == needed)
                        this.ApplyUpdate(update);

                    this.receptionCounts[update.Epoch] = count;
                }
            }


            return true;
        }

        private void ApplyUpdate(LeaseUpdateRequest update)
        {
            Logger.GetInstance().Log("LeaseUpdateService.ApplyUpdate", $"Got a majority so add it to updates buffer");
            this.transactionManager.Leasing.LeaseReceptionBuffer.Add(update);

            // TODO when can we garbage collect this?
            // this.receptionCounts.Remove(update.Epoch);
        }
    }
}

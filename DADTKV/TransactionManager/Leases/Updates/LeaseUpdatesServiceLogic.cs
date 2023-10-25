using Common;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
            lock (this.transactionManager.Leasing.LeaseReceptionBuffer)
            {
                Logger.GetInstance().Log("LeaseUpdateService", $"Received a Lease update for epoch={update.Epoch}!");

                // We already had a majority, so ignore
                // TODO is this enough? because if we clear the leases, then we're screwed
                if (this.transactionManager.Leasing.LeaseReceptionBuffer.Has(update.Epoch))
                    return true;

                // Check if we have a majority or not
                lock (this)
                {
                    int count = 0;
                    if (this.receptionCounts.ContainsKey(update.Epoch))
                        count = this.receptionCounts[update.Epoch];

                    count++;

                    int needed = this.transactionManager.Leasing.LeaseManagers.Count / 2;
                    Logger.GetInstance().Log("LeaseUpdateService", $"Got {count}/{needed} updates for epoch={update.Epoch}");

                    // First arrival define the start of the new epoch
                    if (count == 1)
                        this.transactionManager.Leasing.Epoch = update.Epoch;

                    if (count == needed)
                        this.ApplyUpdate(update);
                    else
                        this.receptionCounts[update.Epoch] = count;
                }
            }

            return true;
        }

        private void ApplyUpdate(LeaseUpdateRequest update)
        {
            Logger.GetInstance().Log("LeaseUpdateService.ApplyUpdate", $"Got a majority so add it to buffer");

            // We have a majority - its an update
            this.transactionManager.Leasing.LeaseReceptionBuffer.Add(update);
            // this.receptionCounts.Remove(update.Epoch); // TODO RELATED TO THE FIRST TODO 

            // TODO lease reception buffer TO the actual leases

            // Check if there is any lease that's conflicting after executing this transaction --
            List<string> conflictingLeases = this.transactionManager.Leasing.GetOwnedLeases()
                    .Where(lease => this.transactionManager.Leasing.IsConflicting(lease))
                    .ToList();

            // Free them
            // TODO how do we handle wanting to apply a transaction but having to free the lease?
            //if (conflictingLeases.Count > 0)
            // this.leasing.Free(conflictingLeases);
            // --
        }
    }
}

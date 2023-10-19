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
        private Leasing leasing;
        private Dictionary<int, int> receptionCounts = new Dictionary<int, int>();

        public LeaseUpdatesServiceLogic(Leasing leasing)
        {
            this.leasing = leasing;
        }

        public bool LeaseUpdate(LeaseUpdateRequest update)
        {
            lock (this.leasing.LeaseReceptionBuffer)
            {
                // We already had a majority, so ignore
                // TODO is this enough? because if we clear the leases, then we're screwed
                if (this.leasing.LeaseReceptionBuffer.Has(update.Epoch))
                    return true;

                // Check if we have a majority or not
                lock (this)
                {
                    int count = 0;
                    if (this.receptionCounts.ContainsKey(update.Epoch))
                        count = this.receptionCounts[update.Epoch];

                    count++;

                    // First arrival define the start of the new epoch
                    if (count == 1)
                        this.leasing.Epoch = update.Epoch;

                    if (count >= this.leasing.LeaseManagers.Count / 2)
                        this.ApplyUpdate(update);
                    else
                        this.receptionCounts[update.Epoch] = count;
                }
            }

            return true;
        }

        private void ApplyUpdate(LeaseUpdateRequest update)
        {
            // We have a majority - its an update
            this.leasing.LeaseReceptionBuffer.Add(updates);
            this.receptionCounts.Remove(updates.Epoch); // TODO RELATED TO THE FIRST TODO 

            // TODO lease reception buffer TO the actual leases
            //  - check for conflicting leases, if we need to release any
        }
    }
}

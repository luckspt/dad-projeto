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

        public bool LeaseUpdate(LeaseUpdateRequest updates)
        {
            lock (this.leasing.LeaseReceptionBuffer)
            {
                // We already had a majority, so ignore
                if (this.leasing.LeaseReceptionBuffer.Has(updates.Epoch)) return true;

                // Check if we have a majority or not
                lock (this)
                {
                    int count = 0;
                    if (this.receptionCounts.ContainsKey(updates.Epoch)) count = this.receptionCounts[updates.Epoch];

                    count++;
                    // Do we have majority?
                    if (count >= this.leasing.LeaseManagers.Count / 2)
                    {
                        this.leasing.LeaseReceptionBuffer.Add(updates);
                        this.receptionCounts.Remove(updates.Epoch);
                    }
                    else
                    {
                        this.receptionCounts[updates.Epoch] = count;
                    }
                }
            }

            return true;
        }
    }
}

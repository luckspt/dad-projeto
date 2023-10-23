using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Leases
{
    /// <summary>
    /// Store a buffer of the received leases for each epoch
    /// So that the TM sees the leases sorted by epoch and not at random
    /// 
    /// It is ordered by epoch
    /// </summary>
    internal class LeaseReceptionBuffer
    {
        private SortedList<int, LeaseUpdates.LeaseUpdateRequest> buffer;

        public void Add(LeaseUpdates.LeaseUpdateRequest update)
        {
            // TODO should we check if its the last?
            this.buffer.Add(update.Epoch, update);
        }

        public bool Has(int epoch)
        {
            return this.buffer.ContainsKey(epoch);
        }

        public void Remove(int epoch)
        {
            this.buffer.Remove(epoch);
        }
    }
}

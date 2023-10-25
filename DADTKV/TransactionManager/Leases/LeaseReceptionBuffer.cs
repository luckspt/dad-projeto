using Common;
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
        private Leasing leasing;
        private SortedList<int, LeaseUpdates.LeaseUpdateRequest> buffer;
        private int lastUpdatedEpoch;

        public LeaseReceptionBuffer(Leasing leasing)
        {
            this.lastUpdatedEpoch = 0;
            this.leasing = leasing;
            this.buffer = new SortedList<int, LeaseUpdates.LeaseUpdateRequest>();
        }

        public void Add(LeaseUpdates.LeaseUpdateRequest update)
        {
            lock (this)
            {
                lock (this.leasing)
                {
                    Logger.GetInstance().Log("LeaseReceptionBuffer.Add", $"Im at lastUpdatedEpoch={this.lastUpdatedEpoch} and I received an update for epoch={update.Epoch}");

                    // Check if we update to the next epoch
                    if (this.lastUpdatedEpoch + 1 == update.Epoch)
                    {
                        // Update leasing without going by the buffer
                        LeaseUpdates.LeaseUpdateRequest toUpdate = update;

                        do
                        {
                            Logger.GetInstance().Log("LeaseReceptionBuffer", $"Applying Leasing for epoch={update.Epoch}");

                            this.leasing.Update(toUpdate.Leases);
                            this.lastUpdatedEpoch++;
                            // Remove
                            this.buffer.Remove(toUpdate.Epoch);

                            // Exhaust all other updates if there's any
                            if (this.buffer.Count != 0)
                                toUpdate = this.buffer.ElementAt(0).Value;
                        } while (this.lastUpdatedEpoch + 1 == toUpdate.Epoch);
                    }
                    else if (!this.buffer.ContainsKey(update.Epoch) && update.Epoch > this.lastUpdatedEpoch)
                        this.buffer.Add(update.Epoch, update);

                    // Pulse so the worker wakes up
                    Monitor.Pulse(this.leasing);
                }
            }
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

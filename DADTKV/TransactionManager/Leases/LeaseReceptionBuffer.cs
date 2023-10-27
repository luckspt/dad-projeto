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
        private TransactionManager transactionManager;
        private SortedList<int, LeaseUpdates.LeaseUpdateRequest> buffer;
        private int lastUpdatedEpoch;

        public LeaseReceptionBuffer(TransactionManager transactionManager)
        {
            this.lastUpdatedEpoch = 0;
            this.transactionManager = transactionManager;
            this.buffer = new SortedList<int, LeaseUpdates.LeaseUpdateRequest>();
        }

        public void Add(LeaseUpdates.LeaseUpdateRequest update)
        {
            lock (this)
            {

                Logger.GetInstance().Log("LeaseReceptionBuffer.Add", $"Im at lastUpdatedEpoch={this.lastUpdatedEpoch} and I received an update for epoch={update.Epoch}");

                // Check if we update to the next epoch
                if (this.lastUpdatedEpoch + 1 == update.Epoch)
                {
                    // Update leasing without going by the buffer
                    LeaseUpdates.LeaseUpdateRequest toUpdate = update;

                    lock (this.transactionManager.Leasing)
                    {
                        do
                        {
                            this.ApplyLeaseUpdate(update);

                            // Exhaust all other updates if there's any
                            if (this.buffer.Count != 0)
                                toUpdate = this.buffer.ElementAt(0).Value;
                        } while (this.lastUpdatedEpoch + 1 == toUpdate.Epoch);

                        // Pulse so the worker wakes up
                        Monitor.PulseAll(this.transactionManager.Leasing);
                    }
                }
                else if (!this.buffer.ContainsKey(update.Epoch) && update.Epoch > this.lastUpdatedEpoch)
                    this.buffer.Add(update.Epoch, update);
            }
        }

        private void ApplyLeaseUpdate(LeaseUpdates.LeaseUpdateRequest update)
        {
            // this and Leasing lock is owned

            Logger.GetInstance().Log("LeaseReceptionBuffer", $"Applying Leasing for epoch={update.Epoch}");

            this.transactionManager.Leasing.Update(update.Leases);
            this.lastUpdatedEpoch++;

            // Populate with all leases I own
            List<string> leasesICanFree = this.transactionManager.Leasing.GetOwnedLeases();

            // Know all leases I need right now (only for the next transaction because it *may* have already requested)
            // This is an optimization so we don't give away leases that will be used right after
            List<string> leasesINeed = new List<string>();
            lock (this.transactionManager.TransactionsBuffer)
            {
                if (this.transactionManager.TransactionsBuffer.Count > 0)
                {
                    Transactions.Transaction currentTransaction = this.transactionManager.TransactionsBuffer.Get(0);
                    leasesINeed.AddRange(currentTransaction.GetLeasesKeys());
                }
            }

            // Make it a set so its O(1) search
            HashSet<string> leasesINeedSet = leasesINeed.ToHashSet();
            // Now we know the leases that won't be used right next
            leasesICanFree.RemoveAll(x => leasesINeedSet.Contains(x));

            // Only need to free conflicting leases
            List<string> leasesToFree = leasesICanFree.Where(x => this.transactionManager.Leasing.IsConflicting(x)).ToList();

            Logger.GetInstance().Log("LeaseReceptionBuffer.ApplyLeaseUpdate", $"Checking if I need to free leases. ICanFree={string.Join(",", leasesICanFree)}, toFree={string.Join(",", leasesToFree)}");

            // Only set to free if there actually leases to be freed
            if (leasesToFree.Count > 0)
            {
                // !!!!!DONT FREE THE LEASES!!!!!
                // MAKE A NO-OP TRANSACTION INSTEAD
                // REPLICATE THAT TRANSACTION (add to buffer head)
                // - THE CONSEQUENCE IS ALL TMs WILL SEE THIS AS A
                //   TRANSACTION AND FREE THE LEASES
                Transactions.Transaction freeLeasesTransaction = new Transactions.Transaction(this.transactionManager.ManagerId, Guid.NewGuid().ToString(),
                    leasesToFree.Select(x => new Transactions.ReadOperation(x)).ToList(), new List<Transactions.WriteOperation>());

                lock (this.transactionManager.TransactionsBuffer)
                {
                    this.transactionManager.TransactionsBuffer.AddToHead(freeLeasesTransaction);
                }
            }
        }

        // Remove
        // TODO when can we garbage collect this?
        // this.buffer.Remove(update.Epoch);


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

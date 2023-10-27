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
            lock (this.transactionManager.Leasing.LeaseReceptionBuffer)
            {
                Logger.GetInstance().Log("LeaseUpdateService", $"Received a Lease update for epoch={update.Epoch}!");

                // We already applied of have had a majority, so ignore
                if (this.transactionManager.Leasing.Epoch > update.Epoch || this.transactionManager.Leasing.LeaseReceptionBuffer.Has(update.Epoch))
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
            lock (this.transactionManager.Leasing)
            {
                Logger.GetInstance().Log("LeaseUpdateService.ApplyUpdate", $"Got a majority so add it to updates buffer");
                this.transactionManager.Leasing.LeaseReceptionBuffer.Add(update);

                // TODO when can we garbage collect this?
                // this.receptionCounts.Remove(update.Epoch);

                lock (this.transactionManager.TransactionsBuffer)
                {
                    // Populate with all leases I own
                    List<string> leasesICanFree = this.transactionManager.Leasing.GetOwnedLeases();

                    // Know all leases I need right now (only for the next transaction because it *may* have already requested)
                    // This is an optimization so we don't give away leases that will be used right after
                    List<string> leasesINeed = new List<string>();
                    if (this.transactionManager.TransactionsBuffer.Count > 0)
                    {
                        Transactions.Transaction currentTransaction = this.transactionManager.TransactionsBuffer.Get(0);
                        leasesINeed.AddRange(currentTransaction.GetLeasesKeys());
                    }

                    // Make it a set so its O(1) search
                    HashSet<string> leasesINeedSet = leasesINeed.ToHashSet();
                    // Now we know the leases that won't be used right next
                    leasesICanFree.RemoveAll(x => leasesINeedSet.Contains(x));

                    // Only need to free conflicting leases
                    List<string> leasesToFree = leasesICanFree.Where(x => this.transactionManager.Leasing.IsConflicting(x)).ToList();

                    // !!!!!DONT FREE THE LEASES!!!!!
                    // MAKE A NO-OP TRANSACTION INSTEAD
                    // REPLICATE THAT TRANSACTION (add to buffer head)
                    // - THE CONSEQUENCE IS ALL TMs WILL SEE THIS AS A
                    //   TRANSACTION AND FREE THE LEASES
                    Transactions.Transaction freeLeasesTransaction = new Transactions.Transaction(this.transactionManager.ManagerId, Guid.NewGuid().ToString(),
                        leasesToFree.Select(x => new Transactions.ReadOperation(x)).ToList(), new List<Transactions.WriteOperation>());

                    this.transactionManager.TransactionsBuffer.AddToHead(freeLeasesTransaction);
                }
            }
        }
    }
}

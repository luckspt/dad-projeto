using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManager.Leases.LeaseRequesting;

namespace TransactionManager.Leases
{
    internal class Leasing
    {
        public int Epoch { get; set; }
        public int NextEpoch
        {
            get => this.Epoch + 1;
        }

        public List<Peer> LeaseManagers { get; }
        public LeaseReceptionBuffer LeaseReceptionBuffer { get; }
        public LeaseRequestingServiceClient RequestingServiceClient { get; }

        // This transaction manager
        private TransactionManager transactionManager;
        // The keys and a queue of transaction managers that hold a lease
        private Dictionary<string, List<string>> leases;

        public Leasing(TransactionManager transactionManager, List<Peer> leaseManagers)
        {
            this.transactionManager = transactionManager;
            this.LeaseManagers = leaseManagers;
            this.Epoch = 0;
            this.leases = new Dictionary<string, List<string>>();
            this.LeaseReceptionBuffer = new LeaseReceptionBuffer(transactionManager);
            this.RequestingServiceClient = new LeaseRequestingServiceClient(this);
        }

        public List<string> GetOwnedLeases()
        {
            return this.leases
                .Where(lease => this.HasLease(lease.Key))
                .Select(lease => lease.Key)
                .ToList();
        }

        public bool IsConflicting(string key)
        {
            // contract programming, so requires this.hasLease(key)
            return this.leases[key].Count > 1;
        }

        public bool HasLease(string key, string ownerId)
        {
            return this.leases.ContainsKey(key) &&
                this.leases[key].Count != 0 &&
                this.leases[key][0].Equals(ownerId);
        }

        public bool HasLease(string key)
        {
            return this.HasLease(key, this.transactionManager.ManagerId);
        }

        public bool HasLeases(List<string> keys)
        {
            return keys.All(key => this.HasLease(key));
        }

        public bool Request(List<string> keys)
        {
            return this.RequestingServiceClient.RequestLeases(new LeaseRequesting.RequestLeasesRequest()
            {
                RequesterTMId = this.transactionManager.ManagerId,
                LeaseKeys = keys,
            });
        }

        public bool Update(Dictionary<string, List<string>> newKeys)
        {
            // Updates are received asynchronously, so better to lock it
            lock (this)
            {
                foreach (string key in newKeys.Keys)
                {
                    if (this.leases.ContainsKey(key))
                        this.leases[key].AddRange(newKeys[key]);
                    else
                        this.leases.Add(key, newKeys[key]);
                }
            }

            return true;
        }

        public void Free(List<string> keys, string ownerId)
        {
            lock (this)
            {
                foreach (string key in keys)
                {
                    if (this.leases.ContainsKey(key) && this.leases[key][0].Equals(ownerId))
                        this.leases[key].RemoveAt(0);
                }
            }
        }

        public void Free(List<string> keys)
        {
            this.Free(keys, this.transactionManager.ManagerId);
        }
    }
}

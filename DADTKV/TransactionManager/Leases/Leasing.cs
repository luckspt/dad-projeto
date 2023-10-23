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

        // The id of this transaction manager
        private string managerId;
        // The keys and a queue of transaction managers that hold a lease
        private Dictionary<string, List<string>> leases;

        public Leasing(string managerId, List<Peer> leaseManagers)
        {
            this.managerId = managerId;
            this.LeaseManagers = leaseManagers;
            this.leases = new Dictionary<string, List<string>>();
            this.LeaseReceptionBuffer = new LeaseReceptionBuffer();
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
            return leases[key].Count > 1;
        }

        public bool HasLease(string key)
        {
            return leases[key].Count != 0 &&
                leases[key][0].Equals(this.managerId);
        }

        public bool HasLeases(List<string> keys)
        {
            return keys.All(key => this.HasLease(key));
        }

        public bool Request(List<string> keys)
        {
            return this.RequestingServiceClient.RequestLeases(new LeaseRequesting.RequestLeasesRequest()
            {
                RequesterTMId = this.managerId,
                LeaseKeys = keys,
            });
        }

        public bool Update(Dictionary<string, List<string>> newKeys)
        {
            // TODO: check and free lease if needed
            // Updates are received asynchronously, so better to lock it
            lock (this)
            {
                foreach (string key in newKeys.Keys)
                {
                    // TODO: should we copy or just reference?
                    if (leases.ContainsKey(key))
                    {
                        leases[key].AddRange(newKeys[key]);
                    }
                    else
                    {
                        leases.Add(key, newKeys[key]);
                    }
                }
            }

            return true;
        }

        public void Free(List<string> keys)
        {
            // TODO: implement
            // TODO: if it holds a lease that conflicts with another transaction manager, free it after executing the transaction.
            // - otherwise, it can keep it indefinitely
            lock (this)
            {
                foreach (string key in keys)
                {
                    if (leases.ContainsKey(key))
                    {
                        leases[key].Remove(this.managerId);
                    }
                }
            }
        }
    }
}

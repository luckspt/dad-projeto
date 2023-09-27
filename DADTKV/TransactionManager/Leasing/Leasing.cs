using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Leasing
{
    internal class Leasing
    {
        // The id of this transaction manager
        private string managerId;
        // The keys that are currently leased to this transaction manager
        private HashSet<string> leases;

        public Leasing(string managerId)
        {
            this.managerId = managerId;
            leases = new HashSet<string>();
        }

        public bool hasLease(string key)
        {
            return leases.Contains(key);
        }

        public bool Request(List<string> keys)
        {
            // TODO: implement
            // TODO: request keys that are not leased already to the manager from ALL lease managers
            return true;
        }

        public bool Receive(List<string> newKeys)
        {
            leases.Concat(newKeys);
            return true;
        }

        public void Free(List<string> keys)
        {
            // TODO: implement
            // TODO: if it holds a lease that coflicts with another transaction manager, free it after executing the transaction.
            // - otherwise, it can keep it indefinitely
        }
    }
}

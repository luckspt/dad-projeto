using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager
{
    internal class Lease
    {
        // The id of this transaction manager
        private String managerId;
        // The keys that are currently leased to this transaction manager
        private HashSet<String> leasedKeys;

        public Lease(String managerId)
        {
            this.managerId = managerId;
            this.leasedKeys = new HashSet<String>();
        }

        public bool Request(List<String> keys)
        {
            // TODO: implement
            // TODO: request keys that are not leased already to the manager from ALL lease managers
            return true;
        }

        public void Free(List<String> keys)
        {
            // TODO: implement
            // TODO: if it holds a lease that coflicts with another transaction manager, free it after executing the transaction.
            // - otherwise, it can keep it indefinitely
        }
    }
}

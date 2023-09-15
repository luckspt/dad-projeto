using DADTKV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager
{
    internal class TransactionManager
    {
        // The id of this transaction manager
        private String managerId;
        // Leasing service
        private Lease lease;
        // Key-value store
        private Dictionary<String, int> kvStore;

        public TransactionManager(String managerId)
        {
            this.managerId = managerId;
            this.lease = new Lease(managerId);
        }

        public List<DadInt> Read(String clientId, List<String> keysToRead)
        {
            // TODO: implement
            return null;
        }

        public List<DadInt> Write(String clientId, List<DadInt> toWrite)
        {
            // TODO: implement
            // TODO: propagate to other transaction managers
            return null;
        }
    }
}

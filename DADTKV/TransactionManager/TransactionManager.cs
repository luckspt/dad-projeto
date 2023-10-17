using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManager.Leases;

namespace TransactionManager
{
    public class TMPeer
    {
        public string Address { get; }

        public TMPeer(string address)
        {
            this.Address = address;
        }
    }
    internal class TransactionManager
    {
        // The id of this transaction manager
        private String managerId;
        // Leasing service
        public Leasing Leasing { get; private set; }
        public List<Peer> TransactionManagers { get; private set; }

        // Key-value store
        private Dictionary<String, int> kvStore;

        public TransactionManager(String managerId)
        {
            this.managerId = managerId;
        }

        public void Start(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses)
        {
            this.Leasing = new Leasing(this.managerId, leaseManagersAddresses.Select(address => new Peer(address)).ToList());
            this.TransactionManagers = transactionManagersAddresses.Select(address => new Peer(address)).ToList();
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

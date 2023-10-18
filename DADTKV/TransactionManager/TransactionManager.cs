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

            Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    this.SimulateRequests();
                    Thread.Sleep(1000);
                }
            });
        }

        public void SimulateRequests()
        {
            // generate a random list of 1 to 5 keys non-numeric keys
            Random random = new Random();
            int nKeys = random.Next(1, 6);
            List<string> keys = new List<string>();
            for (int i = 0; i < nKeys; i++)
            {
                keys.Add("key" + random.Next(0, 1000));
            }

            Logger.GetInstance().Log("SimulateRequests", $"Requesting leases (leases=[{string.Join(", ", keys)}])");
            this.Leasing.Request(keys);
        }
    }
}

using LeaseManager.Leases;
using LeaseManager.Paxos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager
{
    internal class LeaseManager
    {
        private string id;
        private LeaseBuffer leaseBuffer;

        public LeaseManager(string id)
        {
            this.leaseBuffer = new LeaseBuffer();
        }

        public void StartPaxos()
        {
            lock (this.leaseBuffer)
            {
                PaxosInstance instance = new PaxosInstance(this.id, this.leaseBuffer.GetBuffer(), 0, 0);
                
            }

        }
    }
}

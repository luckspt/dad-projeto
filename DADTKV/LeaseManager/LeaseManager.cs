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
            Timer timer = new Timer(this.StartPaxos, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public void StartPaxos(object state)
        {
            lock (this.leaseBuffer)
            {
                try
                {
                    PaxosInstance instance = new PaxosInstance(this.id, this.leaseBuffer.GetBuffer(), 0, 0);
                    this.leaseBuffer.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

        }
    }
}

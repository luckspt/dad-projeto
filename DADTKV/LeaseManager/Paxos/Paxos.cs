using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    public class Lease
    {
        public String Key { get; }
        public int Epoch { get; }
        public int TargetTMId { get; }

        public Lease(String key, int epoch, int targetTMId)
        {
            this.Key = key;
            this.Epoch = epoch;
            this.TargetTMId = targetTMId;
        }
    }

    public class Paxos
    {
        public int Id { get; }
        public int Epoch { get; }
        public List<Lease> Value { get; set; }
        public int WriteTimestamp { get; set; }
        public int ReadTimestamp { get; set; }

        public List<int> peers = new List<int>();
    }
}
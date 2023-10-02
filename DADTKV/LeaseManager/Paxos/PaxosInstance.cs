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

    public class PaxosInstance
    {
        // TODO: epoch?
        public string Id { get; }
        private Dictionary<string, List<string>> value;
        private int writeTimestamp;
        private int readTimestamp;
        public Phase1 Phase1 { get; }
        public Phase2 Phase2 { get; }


        public List<int> peers = new List<int>();

        public PaxosInstance(string id, Dictionary<string, List<string>> value, int writeTimestamp, int readTimestamp)
        {
            this.Id = id;
            this.value = value;
            this.writeTimestamp = writeTimestamp;
            this.readTimestamp = readTimestamp;
            this.Phase1 = new Phase1(this);
            this.Phase2 = new Phase2(this);
        }

        public Dictionary<string, List<string>> Value
        {
            get => this.value;
            set
            {
                lock (this)
                {
                    this.value = value;
                }
            }
        }

        public int WriteTimestamp
        {
            get => this.writeTimestamp;
            set
            {
                lock (this)
                {
                    this.writeTimestamp = value;
                }
            }
        }

        public int ReadTimestamp
        {
            get => this.readTimestamp;
            set
            {
                lock (this)
                {
                    this.readTimestamp = value;
                }
            }
        }

        public List<int> GetLearners()
        {
            return this.peers;
        }

        public List<int> GetProposers()
        {
            return this.peers;
        }

        public List<int> GetAcceptors()
        {
            return this.peers;
        }
    }
}
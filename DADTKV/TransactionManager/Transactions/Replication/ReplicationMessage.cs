using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Transactions.Replication
{
    internal class ReplicationMessage
    {
        public string Guid { get; set; }
        public string ExecutingManagerId { get; set; }
        public List<RPCStoreDadInt> DadInts { get; set; } = new List<RPCStoreDadInt>(); // Force them to, at least, be an empty list
        public List<string> ReadDadInts { get; set; } = new List<string>(); // Force them to, at least, be an empty list

        public ReplicationMessage() { }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            ReplicationMessage other = (ReplicationMessage)obj;

            if (!this.Guid.Equals(other.Guid)) return false;
            if (!this.ExecutingManagerId.Equals(other.ExecutingManagerId)) return false;
            if (!this.DadInts.All(other.DadInts.Contains) || this.DadInts.Count != other.DadInts.Count) return false;
            if (!this.ReadDadInts.All(other.ReadDadInts.Contains) || this.ReadDadInts.Count != other.ReadDadInts.Count) return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + this.Guid.GetHashCode();
                hash = (hash * 7) + this.ExecutingManagerId.GetHashCode();

                foreach (var dadInt in this.DadInts)
                    hash = (hash * 7) + (dadInt == null ? 0 : dadInt.GetHashCode());

                foreach (var dadInt in this.ReadDadInts)
                    hash = (hash * 7) + (dadInt == null ? 0 : dadInt.GetHashCode());

                return hash;
            }
        }
    }

    internal class RPCStoreDadInt
    {
        public string Key { get; set; }
        public int Value { get; set; }
        public int Epoch { get; set; }
        public int EpochWriteVersion { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            RPCStoreDadInt other = (RPCStoreDadInt)obj;

            if (!this.Key.Equals(other.Key)) return false;
            if (this.Value != other.Value) return false;
            if (this.Epoch != other.Epoch) return false;
            if (this.EpochWriteVersion != other.EpochWriteVersion) return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + this.Key.GetHashCode();
                hash = (hash * 7) + this.Value.GetHashCode();
                hash = (hash * 7) + this.Epoch.GetHashCode();
                hash = (hash * 7) + this.EpochWriteVersion.GetHashCode();

                return hash;
            }
        }
    }
}

using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Transactions.Replication
{
    internal class BroadcastMessage
    {
        public int OriginReplyLockHash;
        public List<RPCStoreDadInt> DadInts;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            BroadcastMessage other = (BroadcastMessage)obj;

            if (this.OriginReplyLockHash != other.OriginReplyLockHash) return false;
            if (!this.DadInts.All(other.DadInts.Contains) || this.DadInts.Count != other.DadInts.Count) return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + this.OriginReplyLockHash.GetHashCode();

                foreach (var dadInt in this.DadInts)
                {
                    hash = (hash * 7) + (dadInt == null ? 0 : dadInt.GetHashCode());
                }

                return hash;
            }
        }
    }

    internal class RPCStoreDadInt
    {
        public string Key;
        public int Value;
        public int Epoch;
        public int EpochWriteVersion;

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

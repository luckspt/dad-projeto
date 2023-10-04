using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Leases
{
    internal class LeaseBuffer
    {
        private Dictionary<string, List<string>> buffer;

        public LeaseBuffer()
        {
            this.buffer = new Dictionary<string, List<string>>();
        }

        public bool AddRange(string managerId, List<string> keys)
        {
            lock (this)
            {
                foreach (string key in keys)
                {
                    if (this.buffer.ContainsKey(key))
                    {
                        // https://stackoverflow.com/a/12172412
                        int idx = this.buffer[key].BinarySearch(managerId);
                        if (idx < 0) idx = ~idx;

                        this.buffer[key].Insert(idx, managerId);
                    }
                    else
                    {
                        this.buffer.Add(key, new List<string> { managerId });
                    }
                }
            }

            return true;
        }

        public Dictionary<string, List<string>> GetBuffer()
        {
            return this.buffer.ToDictionary(entry => entry.Key, entry => new List<string>(entry.Value));
        }

        public void Clear()
        {
            this.buffer.Clear();
        }
    }
}

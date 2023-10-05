using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.LeaseRequesting
{
    internal class LeaseRequestsBuffer
    {
        private Dictionary<string, List<string>> buffer;

        public LeaseRequestsBuffer()
        {
            this.buffer = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Add keys to the buffer with insertion-sort
        /// </summary>
        /// <param name="managerId">The manager the keys belong to</param>
        /// <param name="keys">The keys</param>
        public void AddRange(string managerId, List<string> keys)
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
        }

        /// <summary>
        /// A copy of the buffer
        /// </summary>
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

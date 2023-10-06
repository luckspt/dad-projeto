using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.LeaseRequesting
{
    internal class LeaseRequestsBuffer
    {
        private LeaseStore buffer;

        public LeaseRequestsBuffer()
        {
            this.buffer = new LeaseStore();
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
        public LeaseStore GetBuffer()
        {
            return this.buffer.Copy();
        }

        public void Clear()
        {
            this.buffer.Clear();
        }
    }
}

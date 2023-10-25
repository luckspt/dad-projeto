using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Leasing.Requesting
{
    internal class LeaseRequestsBuffer
    {
        private LeaseStore buffer;

        public LeaseRequestsBuffer()
        {
            buffer = new LeaseStore();
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
                    if (buffer.ContainsKey(key))
                    {
                        // https://stackoverflow.com/a/12172412
                        int idx = buffer[key].BinarySearch(managerId);
                        if (idx < 0) idx = ~idx;

                        buffer[key].Insert(idx, managerId);
                    }
                    else
                    {
                        buffer.Add(key, new List<string> { managerId });
                    }
                }
            }
        }

        /// <summary>
        /// A copy of the buffer
        /// </summary>
        public LeaseStore GetBuffer()
        {
            return buffer.Copy();
        }

        public void Clear()
        {
            buffer.Clear();
        }
    }
}

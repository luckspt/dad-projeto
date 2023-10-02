﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Leasing
{
    internal class Leasing
    {
        // The id of this transaction manager
        private string managerId;
        // The keys and a queue of transaction managers that hold a lease
        private Dictionary<string, List<string>> leases;

        public Leasing(string managerId)
        {
            this.managerId = managerId;
            leases = new Dictionary<string, List<string>>();
        }

        public bool HasToFree(string key)
        {
            // contract programming, so requires this.hasLease(key)
            return leases[key].Count > 1;
        }

        public bool HasLease(string key)
        {
            return leases.ContainsKey(key) &&
                leases[key][0].Equals(this.managerId);
        }

        public bool HasLeases(List<string> keys)
        {
            return keys.All(key => this.hasLease(key));
        }

        public bool Request(List<string> keys)
        {
            // TODO: implement
            // TODO: contract programming, so no need to validate if we already have the keys
            // TODO: request to ALL lease managers; need to add a uid because of broadcast?
            return true;
        }

        public bool Update(Dictionary<string, List<string>> newKeys)
        {
            // Updates are received asynchronously, so better to lock it
            lock (this)
            {
                foreach (string key in newKeys.Keys)
                {
                    // TODO: should we copy or just reference?
                    if (leases.ContainsKey(key))
                    {
                        leases[key].AddRange(newKeys[key]);
                    }
                    else
                    {
                        leases.Add(key, newKeys[key]);
                    }
                }
            }

            return true;
        }

        public void Free(List<string> keys)
        {
            // TODO: implement
            // TODO: if it holds a lease that conflicts with another transaction manager, free it after executing the transaction.
            // - otherwise, it can keep it indefinitely
            lock (this)
            {
                foreach (string key in keys)
                {
                    if (leases.ContainsKey(key))
                    {
                        leases[key].Remove(this.managerId);
                    }
                }
            }
        }
    }
}

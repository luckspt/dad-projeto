using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Peer
    {
        public string Address { get; }

        public Peer(string address)
        {
            this.Address = address;
        }
    }
}

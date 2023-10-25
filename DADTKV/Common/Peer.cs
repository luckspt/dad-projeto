using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Peer
    {
        public string Id { get; }
        public string Address { get; }

        public Peer(string id, string address)
        {
            this.Id = id;
            this.Address = address;
        }

        public string FullRepresentation()
        {
            return $"{this.Id}={this.Address}";
        }

        public override string ToString()
        {
            return this.Address;
        }

        public static Peer FromString(string raw)
        {
            string[] split = raw.Split("=");
            return new Peer(split[0], split[1]);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DADTKV
{
    public class HostPort
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public HostPort(string host, int port)
        {
            this.Host = host;
            this.Port = port;
        }

        public override string ToString()
        {
            return $"http://{this.Host}:{this.Port}";
        }

        public static HostPort FromString(string hostPortString)
        {
            string withoutProtocol = hostPortString.Replace("http://", "");
            string[] hostPort = withoutProtocol.Split(':');
            return new HostPort(hostPort[0], int.Parse(hostPort[1]));
        }
    }
}

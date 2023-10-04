using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager
{
    public class Pair<F, S>
    {
        public F First;
        public S Second;

        public Pair(F first, S second)
        {
            this.First = first;
            this.Second = second;
        }
    }
}

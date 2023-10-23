// DadIntValue is also defined on DADTKV. Beware when changing.
global using DadIntValue = System.Int32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Store
{
    public class StoreDadInt
    {
        public DadIntValue Value { get; set; }

        // TODO when do we use this?
        // TODO when do we change this? Make them setters?
        public int LastWriteEpoch { get; }
        public int LastWriteTimestamp { get; }
        public string LastWriteTM { get; }
    }
}

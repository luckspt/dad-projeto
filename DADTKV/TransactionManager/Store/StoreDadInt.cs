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

        public int LastWriteEpoch { get; set; }
        public int EpochWriteVersion { get; set; }
    }
}

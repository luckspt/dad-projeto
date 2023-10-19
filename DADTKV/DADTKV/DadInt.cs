using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DADTKV
{
    public class DadInt
    {
        public string Key { get; }
        public int Value { get; set; }

        public DadInt(string key, int value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Key}={Value}";
        }

        public static DadInt CreateAborted()
        {
            return new DadInt("abort", 0);
        }
    }
}

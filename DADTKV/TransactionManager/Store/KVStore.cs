using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Store
{
    public class KVStore : Dictionary<string, StoreDadInt>
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public KVStore Copy()
        {
            KVStore copy = new KVStore();
            foreach (KeyValuePair<string, StoreDadInt> kvp in this)
                copy.Add(kvp.Key, kvp.Value);

            return copy;
        }

        public static KVStore FromDict(Dictionary<string, StoreDadInt> dict)
        {
            KVStore store = new KVStore();
            foreach (KeyValuePair<string, StoreDadInt> kvp in dict)
                store.Add(kvp.Key, kvp.Value);

            return store;
        }

        public string GetSHA254Hash()
        {
            return Encoding.UTF8.GetString(SHA256.Create().ComputeHash(this.ToByteArray()));
        }

        public byte[] ToByteArray()
        {
            if (this == null) return new byte[0];

            // https://stackoverflow.com/a/66106760
            var asString = JsonConvert.SerializeObject(this, SerializerSettings);
            return Encoding.Unicode.GetBytes(asString);
        }

        public override string ToString()
        {
            return "{" + string.Join("; ", this.Select(kv => kv.Key + "=" + string.Join(",", kv.Value)).ToArray()) + "}";
        }

        public string KeysToString()
        {
            return "[" + string.Join("; ", this.Select(kv => kv.Key).ToArray()) + "]";
        }
    }
}

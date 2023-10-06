using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common
{
    public class LeaseStore : Dictionary<string, List<string>>
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public LeaseStore Copy()
        {
            LeaseStore copy = new LeaseStore();
            foreach (KeyValuePair<string, List<string>> kvp in this)
                copy.Add(kvp.Key, kvp.Value.ToList());

            return copy;
        }

        public static LeaseStore FromDict(Dictionary<string, List<string>> dict)
        {
            LeaseStore store = new LeaseStore();
            foreach (KeyValuePair<string, List<string>> kvp in dict)
            {
                store.Add(kvp.Key, kvp.Value);
            }

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
    }
}

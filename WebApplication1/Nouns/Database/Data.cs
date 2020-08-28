using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Database
{
    public class Data
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string MetaData { get; set; }
        public Data()
        {
        }

        public Data(string key, string type, string meta, string value)
        {
            this.Name = key;
            this.Value = value;
            this.Type = type;
            this.MetaData = meta;
        }

        public Data Clone()
        {
            return (Data)this.MemberwiseClone();
        }

        public override string ToString()
        {
            return $"{Name}=>{Value}";
        }
    }
}

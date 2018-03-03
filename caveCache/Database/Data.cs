using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.Database
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
            return string.Format("{1}=>{2}", Name, Value);
        }

        public static void OnSubModelCreating<T>(EntityTypeBuilder<T> tbl) where T : Data
        {
            tbl.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(64);
            tbl.Property(t => t.Value)
                .IsRequired()
                .HasColumnType("tinytext");
            tbl.Property(t => t.Type)
                .IsRequired()
                .HasColumnType("tinytext");
            tbl.Property(t => t.MetaData)
                .IsRequired()
                .HasColumnType("text");
        }
    }
}

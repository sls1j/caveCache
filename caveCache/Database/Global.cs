using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.Database
{
    static class Globals
    {
       public static readonly string IsBootStrapped = "IsBootStrapped";
    }
    class Global
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<Global>();
            tbl.ToTable("Global");
            tbl.HasKey("Key");
            tbl.Property(t => t.Key)
                .IsRequired()
                .HasMaxLength(128);
            tbl.Property(t => t.Value)
                .IsRequired()
                .HasColumnType("text");
        }
    }
}

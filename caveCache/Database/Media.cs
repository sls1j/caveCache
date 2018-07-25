using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.Database
{
    class Media
    {
        public int MediaId { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public int FileSize { get; set; }
        public int AttachId { get; set; }
        public string AttachType { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<Media>();

            tbl.HasKey("MediaId");

            tbl.Property(t => t.FileName)
                .IsRequired()
                .HasMaxLength(1024);

            tbl.Property(t => t.MimeType)
                .IsRequired()
                .HasMaxLength(256);

            tbl.Property(t => t.FileSize)
                .IsRequired();

            tbl.Property(t => t.AttachId)
                .IsRequired();

            tbl.Property(t => t.AttachType)
                .IsRequired();
        }

        public Media Clone()
        {
            return MemberwiseClone() as Media;
        }
    }  
}

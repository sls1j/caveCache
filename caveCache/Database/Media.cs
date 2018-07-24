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
        }

        public Media Clone()
        {
            return MemberwiseClone() as Media;
        }
    }

    class MediaBody
    {
        public int MediaId { get; set; }
        public byte[] Body { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<MediaBody>();
            tbl.ToTable("MediaBody");
            tbl.HasKey("MediaId");
            tbl.Property(t => t.Body)
                .IsRequired()
                .HasColumnType("mediumblob");
        }
    }


    class CaveMedia
    {
        public int CaveId { get; set; }
        public int MediaId { get; set; }

        public Cave Cave { get; set; }
        public Media Media { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<CaveMedia>();
            tbl.ToTable("CaveMedia");
            tbl.HasKey(t => new { t.CaveId, t.MediaId });
        }
    }

    class UserMedia
    {
        public int UserId { get; set; }
        public int MediaId { get; set; }

        public User User { get; set; }
        public Media Media { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<UserMedia>();
            tbl.ToTable("UserMedia");
            tbl.HasKey(t => new { t.UserId, t.MediaId });
        }
    }

    class MediaSetSession
    {
        public int MediaId { get; set; }
        public string SessionId { get; set; }
        public DateTime ExpireTime { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<MediaSetSession>();
            tbl.ToTable("MediaSetSessions");
            tbl.HasKey(m => m.MediaId);
            tbl.Property(m => m.SessionId)
                .IsRequired()
                .HasMaxLength(25);
            tbl.Property(m => m.ExpireTime)
                .IsRequired()
                .HasColumnType("datetime");
        }
    }
}

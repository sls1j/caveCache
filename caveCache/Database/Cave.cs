using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.Database
{
    public class Cave
    {
        public int CaveId { get; set; }
        public bool Saved { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? LocationId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DateDeleted { get; set; }
        public ICollection<CaveData> Data { get; set; }
        public ICollection<CaveLocation> Locations { get; set; }

        public Cave()
        {
        }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<Cave>();
            tbl.ToTable("Cave");
            tbl.HasKey("CaveId");
            tbl.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(128);
            tbl.Property(t => t.Description)
                .IsRequired()
                .HasColumnType("mediumtext");

            CaveData.OnModelCreating(mb);
        }
    }

    public class CaveData : Data
    {
        public int CaveId { get; set; }
        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<CaveData>();
            tbl.HasKey(t => new { t.CaveId, t.Name });
            Data.OnSubModelCreating(tbl);
        }
    }

    public class CaveUser
    {
        public int CaveId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public Cave Cave { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<CaveUser>();
            tbl.ToTable("CaveUser");
            tbl.HasKey(cu => new { cu.UserId, cu.CaveId });
        }
    }

    public class CaveLocation
    {
        public int CaveId { get; set; }
        public int LocationId { get; set; }
        public DateTime? CaptureDate { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal? Altitude { get; set; }
        public decimal? Accuracy { get; set; }
        public decimal? AltitudeAccuracy { get; set; }
        public string Unit { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<CaveLocation>();
            tbl.ToTable("CaveLocation");
            tbl.HasKey( t => new { t.CaveId, t.LocationId });
            tbl.Property(t => t.Latitude)
                .IsRequired()
                .HasColumnType("decimal(11,8)");
            tbl.Property(t => t.Longitude)
                .IsRequired()
                .HasColumnType("decimal(11,8)");
            tbl.Property(t => t.Altitude)
                .HasColumnType("decimal(11,2)");
            tbl.Property(t => t.Accuracy)
                .HasColumnType("decimal(6,0)");
            tbl.Property(t => t.AltitudeAccuracy)
                .HasColumnType("decimal(6,0)");
            tbl.Property(t => t.Unit)
                .IsRequired()
                .HasColumnType("enum('Emperial','Metric')");
            tbl.Property(t => t.Source)
                .IsRequired()
                .HasColumnType("text");
            tbl.Property(t => t.Notes)
                .IsRequired()
                .HasColumnType("mediumText");
        }
    }

    public static class Units
    {
        public const string Emprial = "Emperial";
        public const string Metric = "Metric";
    }
}

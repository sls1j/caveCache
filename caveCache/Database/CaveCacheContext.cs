using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace caveCache.Database
{
    class CaveCacheContext : DbContext
    {
        public DbSet<Cave> Caves { get; set; }
        
        public DbSet<CaveUser> CaveUsers { get; set; }
        public DbSet<CaveMedia> CaveMedia { get; set; }
        public DbSet<CaveLocation> CaveLocations { get; set; }
        public DbSet<CaveData> CaveData { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserData> UserData { get; set; }
        public DbSet<UserMedia> UserMedia { get; set; }
        public DbSet<UserSession> Sessions { get; set; }
        public DbSet<Global> Globals { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<MediaBody> MediaBody { get; set; }
        public DbSet<MediaSetSession> MediaSetSession { get; set; }

        public string ConnectionString { get; set; }

        public CaveCacheContext()
        {
        }

        public CaveCacheContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var p = new object[] { modelBuilder };
            var asm = Assembly.GetEntryAssembly();
            foreach (var t in asm.GetTypes())
            {
                var method = t.GetMethod("OnModelCreating");
                if (null != method)
                {
                    Console.WriteLine($"Invoking {t.Name}.OnModelCreating");
                    method.Invoke(null, p);
                }
            }
        }
    }
}

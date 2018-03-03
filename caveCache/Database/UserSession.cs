using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace caveCache.Database
{
    public class UserSession
    {
        public int UserId { get; set; }
        public string SessionId { get; set; }
        public DateTime Timeout { get; set; }
        public bool IsCommandLine { get; set; }

        public User User { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<UserSession>();
            tbl.ToTable("UserSession");
            tbl.HasKey(t => t.SessionId);
            tbl.Property(p => p.SessionId)
                .IsRequired()
                .HasMaxLength(24);
        }
    }
}

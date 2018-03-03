using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.Database
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastLoggedIn { get; set; }
        public DateTime? Expire { get; set; }
        public string Profile { get; set; }
        public string Permissions { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }

        public ICollection<UserData> Data { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, UserId);
        }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<User>();
            tbl.ToTable("User");
            tbl.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(128);
            tbl.Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(256);
            tbl.Property(p => p.Profile)
                .IsRequired()
                .HasColumnType("text");
            tbl.Property(p => p.Permissions)
                .IsRequired()
                .HasColumnType("tinytext");
            tbl.Property(p => p.PasswordSalt)
                .IsRequired()
                .HasColumnType("text");
            tbl.Property(p => p.PasswordHash)
                .IsRequired()
                .HasColumnType("text");                            

            UserData.OnModelCreating(mb);
        }
    }

    public class UserData : Data
    {
        public int UserId { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<UserData>();
            tbl.HasKey(t => new { t.UserId, t.Name });
            Data.OnSubModelCreating(tbl);
        }
    }
}

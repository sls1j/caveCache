using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caveCache.Database
{
    class History
    {
        public int HistoryId { get; set; }
        public int? UserId { get; set; }
        public int? CaveId { get; set; }
        public int? SurveyId { get; set; }
        public int? MediaId { get; set; }
        public DateTime EventDateTime { get; set; }
        public string Description { get; set; }
        public string Data { get; set; }

        public static void OnModelCreating(ModelBuilder mb)
        {
            var tbl = mb.Entity<History>();
            tbl.HasKey(t => t.HistoryId);
            tbl.Property(t => t.EventDateTime)
                .IsRequired();
            tbl.Property(t => t.Description)
                .IsRequired()
                .HasColumnType( "text");
            tbl.Property(t => t.Data)
                .IsRequired()
                .HasColumnType("mediumtext");
        }
    }
}

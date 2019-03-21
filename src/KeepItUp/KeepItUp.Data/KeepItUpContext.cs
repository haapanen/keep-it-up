using System;
using System.Collections.Generic;
using System.Text;
using KeepItUp.Core;
using Microsoft.EntityFrameworkCore;

namespace KeepItUp.Data
{
    public class KeepItUpContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=database.sqlite");
        }
    }
}

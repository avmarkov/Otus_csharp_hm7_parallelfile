using Microsoft.EntityFrameworkCore;
using Otus.Teaching.Concurrency.Import.Handler.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Otus.Teaching.Concurrency.Import.DataAccess
{
    public class MyDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        private string _config;

        public MyDbContext(string config)
        {
            _config = config;
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_config);
        }
    }
}

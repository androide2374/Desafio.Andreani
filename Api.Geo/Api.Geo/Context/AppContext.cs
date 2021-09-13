using Api.Geo.Models.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Geo.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Geocodificar> Geocodificar { get; set; }
        public DbSet<PedidoGeo> PedidoGeo { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Geocodificar>()
                    .Property(s => s.Id)
                    .ValueGeneratedOnAdd();
            modelBuilder.Entity<PedidoGeo>()
                    .Property(s => s.Id)
                    .ValueGeneratedOnAdd();
        }
    }
}


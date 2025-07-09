using Microsoft.EntityFrameworkCore;
using EntradaSaida.Core.Models;

namespace EntradaSaida.Infrastructure
{
    /// <summary>
    /// Contexto principal do Entity Framework
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    
        // DbSets
        public DbSet<CounterEvent> CounterEvents { get; set; }
        public DbSet<CountingLine> CountingLines { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        
            // Configurações para CounterEvent
            modelBuilder.Entity<CounterEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.PersonId).IsRequired();
                entity.Property(e => e.PositionX).IsRequired();
                entity.Property(e => e.PositionY).IsRequired();
                entity.Property(e => e.Confidence).IsRequired();
                entity.Property(e => e.CameraId).HasMaxLength(100);
            
                // Índices para performance
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => new { e.Timestamp, e.Type });
            });
        
            // Configurações para CountingLine
            modelBuilder.Entity<CountingLine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StartX).IsRequired();
                entity.Property(e => e.StartY).IsRequired();
                entity.Property(e => e.EndX).IsRequired();
                entity.Property(e => e.EndY).IsRequired();
                entity.Property(e => e.Direction).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.CameraId).HasMaxLength(100);
            });
        }
    }
}

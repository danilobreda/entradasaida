using Microsoft.EntityFrameworkCore;
using EntradaSaida.Core.Models;

namespace EntradaSaida.Infrastructure.Data;

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
        
        // Configurações para SystemConfig
        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // Índice único na chave
            entity.HasIndex(e => e.Key).IsUnique();
        });
        
        // Dados iniciais
        SeedData(modelBuilder);
    }
    
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Configurações padrão
        var defaultConfigs = new List<SystemConfig>
        {
            new() { Id = 1, Key = SystemConfig.Keys.CameraUrl, Value = "0", Type = ConfigType.String, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Key = SystemConfig.Keys.ModelPath, Value = "models/yolov8n.onnx", Type = ConfigType.String, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, Key = SystemConfig.Keys.ConfidenceThreshold, Value = "0.5", Type = ConfigType.Float, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, Key = SystemConfig.Keys.MaxTrackingDistance, Value = "50.0", Type = ConfigType.Float, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, Key = SystemConfig.Keys.TrackingTimeout, Value = "30", Type = ConfigType.Integer, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 6, Key = SystemConfig.Keys.VideoWidth, Value = "640", Type = ConfigType.Integer, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 7, Key = SystemConfig.Keys.VideoHeight, Value = "480", Type = ConfigType.Integer, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 8, Key = SystemConfig.Keys.FrameRate, Value = "30", Type = ConfigType.Integer, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 9, Key = SystemConfig.Keys.EnableRecording, Value = "false", Type = ConfigType.Boolean, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 10, Key = SystemConfig.Keys.RecordingPath, Value = "recordings/", Type = ConfigType.String, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        
        modelBuilder.Entity<SystemConfig>().HasData(defaultConfigs);
        
        // Linha de contagem padrão
        var defaultLine = new CountingLine
        {
            Id = 1,
            Name = "Entrada Principal",
            StartX = 100,
            StartY = 200,
            EndX = 500,
            EndY = 200,
            Direction = LineDirection.TopToBottom,
            IsActive = true,
            CameraId = "default"
        };
        
        modelBuilder.Entity<CountingLine>().HasData(defaultLine);
    }
}

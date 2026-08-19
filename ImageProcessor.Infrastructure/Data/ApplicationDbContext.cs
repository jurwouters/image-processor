using System.Text.Json;
using ImageProcessor.Domain.Entities;
using ImageProcessor.Domain.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ImageProcessor.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new ImageOperationJsonConverter() }
    };

    private static readonly ValueComparer<List<ImageOperation>> OperationsComparer = new(
        (a, b) => JsonSerializer.Serialize(a, _jsonOptions) == JsonSerializer.Serialize(b, _jsonOptions),
        v => JsonSerializer.Serialize(v, _jsonOptions).GetHashCode(),
        v => JsonSerializer.Deserialize<List<ImageOperation>>(JsonSerializer.Serialize(v, _jsonOptions), _jsonOptions)!);

    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<Image> Images => Set<Image>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasMany(e => e.Images)
                  .WithOne(e => e.Batch)
                  .HasForeignKey(e => e.BatchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.S3Key).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.UploadedAt).IsRequired(false);
            entity.Property(e => e.Operations)
                  .HasColumnType("jsonb")
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, _jsonOptions),
                      v => JsonSerializer.Deserialize<List<ImageOperation>>(v, _jsonOptions) ?? new List<ImageOperation>())
                  .Metadata.SetValueComparer(OperationsComparer);
            entity.HasIndex(e => new { e.BatchId, e.S3Key }).IsUnique();
        });
    }
}

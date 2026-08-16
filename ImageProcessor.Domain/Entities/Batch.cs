using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Domain.Entities;

public class Batch
{
    public Guid Id { get; set; }
    public BatchStatus Status { get; set; } = BatchStatus.Created;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<ImageOperation> Operations { get; set; } = [];
    public ICollection<Image> Images { get; set; } = [];
}

public enum BatchStatus
{
    Created,
    Queued,
    Processing,
    Completed,
    Failed
}

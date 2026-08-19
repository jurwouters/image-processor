using ImageProcessor.Domain.Operations;

namespace ImageProcessor.Domain.Entities;

public class Image
{
    public Guid Id { get; set; }
    public required string FileName { get; set; }
    public required string S3Key { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public ImageStatus Status { get; set; } = ImageStatus.PendingUpload;
    public DateTime? UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public List<ImageOperation> Operations { get; set; } = [];

    public Guid BatchId { get; set; }
    public Batch Batch { get; set; } = null!;
}

public enum ImageStatus
{
    PendingUpload,
    Uploaded,
    Processing,
    Completed,
    Failed
}

using ImageProcessor.Web.Models;
using ImageProcessor.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace ImageProcessor.Web.Components.Pages;

public partial class ProcessImages : ComponentBase
{
    [Inject]
    private BatchApiClient BatchApiClient { get; set; } = default!;

    private bool _isProcessing;
    private string? _statusMessage;
    private string? _errorMessage;

    private bool _resizeEnabled;
    private int _resizeWidth = 1024;
    private int _resizeHeight = 768;

    private bool _cropEnabled;
    private int _cropX;
    private int _cropY;
    private int _cropWidth = 500;
    private int _cropHeight = 500;

    private int _batchStatus;
    private CreateBatchResponseDto? _createdBatch;
    private readonly List<IBrowserFile> _selectedFiles = [];
    private readonly List<DownloadItem> _downloadItems = [];

    private void HandleFilesSelected(InputFileChangeEventArgs eventArgs)
    {
        _selectedFiles.Clear();
        _selectedFiles.AddRange(eventArgs.GetMultipleFiles());
    }

    private async Task ProcessBatchAsync()
    {
        _errorMessage = null;
        _statusMessage = null;
        _downloadItems.Clear();

        if (!ValidateInput())
            return;

        _isProcessing = true;

        try
        {
            _statusMessage = "Preparing your job...";

            var createRequest = new CreateBatchRequestDto
            {
                ImagesMetadata = _selectedFiles.Select(file => new ImageMetadataDto
                {
                    FileName = file.Name,
                    ContentType = NormalizeContentType(file.ContentType)
                }).ToArray(),
                Operations = BuildOperations()
            };

            _createdBatch = await BatchApiClient.CreateBatchAsync(createRequest, CancellationToken.None);
            _batchStatus = _createdBatch.Status;

            _statusMessage = "Sending your images...";

            for (var i = 0; i < _selectedFiles.Count; i++)
            {
                var file = _selectedFiles[i];
                var upload = _createdBatch.PresignedUploads[i];

                await using var stream = file.OpenReadStream(file.Size, CancellationToken.None);
                await BatchApiClient.UploadToPresignedUrlAsync(
                    upload.UploadUrl,
                    stream,
                    file.Size,
                    NormalizeContentType(file.ContentType),
                    CancellationToken.None);

                _downloadItems.Add(new DownloadItem(file.Name, BatchApiClient.BuildDownloadUrl(upload.UploadUrl)));
            }

            _statusMessage = "Starting image processing...";

            var started = await BatchApiClient.StartBatchAsync(_createdBatch.Id, CancellationToken.None);
            _batchStatus = started.Status;

            _statusMessage = "Processing started successfully.";
        }
        catch (Exception)
        {
            _errorMessage = "Something went wrong while processing your images. Please try again.";
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private bool ValidateInput()
    {
        if (_selectedFiles.Count == 0)
        {
            _errorMessage = "Please select at least one image.";
            return false;
        }

        return true;
    }

    private IReadOnlyList<ImageOperationDto> BuildOperations()
    {
        var operations = new List<ImageOperationDto>();

        if (_resizeEnabled)
        {
            operations.Add(new ResizeOperationDto
            {
                Width = _resizeWidth,
                Height = _resizeHeight
            });
        }

        if (_cropEnabled)
        {
            operations.Add(new CropOperationDto
            {
                X = _cropX,
                Y = _cropY,
                Width = _cropWidth,
                Height = _cropHeight
            });
        }

        return operations;
    }

    private static string NormalizeContentType(string contentType)
        => string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType;

    private static string FormatSize(long bytes)
        => bytes >= 1024 * 1024
            ? $"{bytes / 1024d / 1024d:F2} MB"
            : $"{bytes / 1024d:F2} KB";

    private static string MapBatchStatus(int status)
        => status switch
        {
            0 => "Created",
            1 => "Waiting",
            2 => "In progress",
            3 => "Completed",
            4 => "Failed",
            _ => $"Unknown ({status})"
        };

    private sealed record DownloadItem(string FileName, string DownloadUrl);
}

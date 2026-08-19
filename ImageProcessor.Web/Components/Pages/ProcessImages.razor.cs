using ImageProcessor.Web.Models;
using ImageProcessor.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace ImageProcessor.Web.Components.Pages;

public partial class ProcessImages : ComponentBase
{
    private const int DefaultResizeWidth = 1024;
    private const int DefaultResizeHeight = 768;
    private const int DefaultCropWidth = 500;
    private const int DefaultCropHeight = 500;

    [Inject]
    private BatchApiClient BatchApiClient { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    private bool _isProcessing;
    private string? _statusMessage;
    private string? _errorMessage;

    private int _batchStatus;
    private int _fileSelectionVersion;
    private readonly DefaultOperationSettings _defaultSettings = new();
    private CreateBatchResponseDto? _createdBatch;
    private readonly List<IBrowserFile> _selectedFiles = [];
    private readonly List<ImageOperationSettings> _selectedImageSettings = [];
    private readonly List<DownloadItem> _downloadItems = [];

    private async Task HandleFilesSelected(InputFileChangeEventArgs eventArgs)
    {
        var currentSelectionVersion = ++_fileSelectionVersion;

        _selectedFiles.Clear();
        _selectedImageSettings.Clear();

        var files = eventArgs.GetMultipleFiles().ToArray();
        _selectedFiles.AddRange(files);

        foreach (var ignoredFile in files)
        {
            _ = ignoredFile;
            _selectedImageSettings.Add(new ImageOperationSettings());
        }

        var dimensionsByFile = await GetSelectedImageDimensionsAsync();

        if (currentSelectionVersion != _fileSelectionVersion)
        {
            return;
        }

        var imageCount = Math.Min(files.Length, _selectedImageSettings.Count);

        for (var index = 0; index < imageCount; index++)
        {
            var dimensions = index < dimensionsByFile.Count ? dimensionsByFile[index] : null;
            if (dimensions is null)
            {
                continue;
            }

            var settings = _selectedImageSettings[index];

            if (dimensions.Width is > 0)
            {
                settings.ResizeWidth = dimensions.Width.Value;
                settings.CropWidth = dimensions.Width.Value;
                settings.SourceWidth = dimensions.Width.Value;
            }

            if (dimensions.Height is > 0)
            {
                settings.ResizeHeight = dimensions.Height.Value;
                settings.CropHeight = dimensions.Height.Value;
                settings.SourceHeight = dimensions.Height.Value;
            }
        }
    }

    private async Task ProcessBatchAsync()
    {
        _errorMessage = null;
        _statusMessage = null;
        _downloadItems.Clear();

        if (!ValidateInput())
        {
            return;
        }

        _isProcessing = true;

        try
        {
            _statusMessage = "Preparing your job...";

            var createRequest = new CreateBatchRequestDto
            {
                Images = _selectedFiles
                    .Select((file, index) => new CreateBatchImageRequestDto
                    {
                        FileName = file.Name,
                        ContentType = NormalizeContentType(file.ContentType),
                        Operations = IsDefaultSettingsEnabled
                            ? BuildOperationsFromDefaultSettings(_defaultSettings, _selectedImageSettings[index])
                            : BuildOperationsFromImageSettings(_selectedImageSettings[index])
                    })
                    .ToArray()
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
            }

            _statusMessage = "Starting image processing...";

            var started = await BatchApiClient.StartBatchAsync(_createdBatch.Id, CancellationToken.None);
            _batchStatus = started.Status;

            await PollBatchUntilFinishedAsync(_createdBatch.Id, CancellationToken.None);
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

    private bool IsDefaultSettingsEnabled
        => _defaultSettings.ResizeEnabled
           || _defaultSettings.CropEnabled
           || _defaultSettings.RotateEnabled;

    private bool ValidateInput()
    {
        if (_selectedFiles.Count == 0)
        {
            _errorMessage = "Please select at least one image.";
            return false;
        }

        if (_selectedImageSettings.Count != _selectedFiles.Count)
        {
            _errorMessage = "Image operation settings are out of sync. Please select files again.";
            return false;
        }

        return true;
    }

    private static IReadOnlyList<ImageOperationDto> BuildOperationsFromImageSettings(ImageOperationSettings settings)
    {
        var operations = new List<ImageOperationDto>();

        if (settings.ResizeEnabled)
        {
            operations.Add(new ResizeOperationDto
            {
                Width = settings.ResizeWidth,
                Height = settings.ResizeHeight
            });
        }

        if (settings.CropEnabled)
        {
            operations.Add(new CropOperationDto
            {
                X = settings.CropX,
                Y = settings.CropY,
                Width = settings.CropWidth,
                Height = settings.CropHeight
            });
        }

        if (settings.RotateEnabled)
        {
            operations.Add(new RotateOperationDto
            {
                Degrees = settings.RotateDegrees
            });
        }

        return operations;
    }

    private static IReadOnlyList<ImageOperationDto> BuildOperationsFromDefaultSettings(
        DefaultOperationSettings defaultSettings,
        ImageOperationSettings imageSettings)
    {
        var operations = new List<ImageOperationDto>();
        var sourceWidth = imageSettings.SourceWidth ?? DefaultResizeWidth;
        var sourceHeight = imageSettings.SourceHeight ?? DefaultResizeHeight;

        if (defaultSettings.ResizeEnabled)
        {
            operations.Add(new ResizeOperationDto
            {
                Width = PercentageToPixels(sourceWidth, defaultSettings.ResizeWidthPercentage),
                Height = PercentageToPixels(sourceHeight, defaultSettings.ResizeHeightPercentage)
            });
        }

        if (defaultSettings.CropEnabled)
        {
            var cropWidth = PercentageToPixels(sourceWidth, defaultSettings.CropWidthPercentage);
            var cropHeight = PercentageToPixels(sourceHeight, defaultSettings.CropHeightPercentage);

            operations.Add(new CropOperationDto
            {
                X = defaultSettings.CropX,
                Y = defaultSettings.CropY,
                Width = Math.Clamp(cropWidth, 1, sourceWidth),
                Height = Math.Clamp(cropHeight, 1, sourceHeight)
            });
        }

        if (defaultSettings.RotateEnabled)
        {
            operations.Add(new RotateOperationDto
            {
                Degrees = defaultSettings.RotateDegrees
            });
        }

        return operations;
    }

    private static int PercentageToPixels(int basePixels, double percentage)
    {
        var clampedPercentage = Math.Max(0.0d, percentage);
        var calculatedPixels = (int)Math.Round(basePixels * (clampedPercentage / 100d));
        return Math.Max(1, calculatedPixels);
    }

    private async Task<IReadOnlyList<ImageDimensions>> GetSelectedImageDimensionsAsync()
    {
        try
        {
            var dimensions = await JsRuntime.InvokeAsync<List<ImageDimensions>>(
                "imageProcessor.getSelectedImageDimensions",
                "image-files-input");

            return dimensions ?? [];
        }
        catch (JSException)
        {
            return [];
        }
    }

    private static string NormalizeContentType(string contentType)
        => string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType;

    private async Task PollBatchUntilFinishedAsync(Guid batchId, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await BatchApiClient.GetBatchStatusAsync(batchId, cancellationToken);
            _batchStatus = batch.Status;

            if (batch.Status == 3)
            {
                _downloadItems.Clear();

                if (_createdBatch is not null)
                {
                    foreach (var upload in _createdBatch.PresignedUploads)
                    {
                        _downloadItems.Add(new DownloadItem(upload.FileName, batchId, upload.Id));
                    }
                }

                _statusMessage = "Processing completed.";
                return;
            }

            if (batch.Status == 4)
            {
                _errorMessage = "Processing failed for one or more images.";
                _statusMessage = null;
                return;
            }

            _statusMessage = "Processing in progress...";
            await InvokeAsync(StateHasChanged);
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

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

    private sealed class DefaultOperationSettings
    {
        public bool ResizeEnabled { get; set; }
        public double ResizeWidthPercentage { get; set; } = 100d;
        public double ResizeHeightPercentage { get; set; } = 100d;
        public bool CropEnabled { get; set; }
        public int CropX { get; set; }
        public int CropY { get; set; }
        public double CropWidthPercentage { get; set; } = 100d;
        public double CropHeightPercentage { get; set; } = 100d;
        public bool RotateEnabled { get; set; }
        public double RotateDegrees { get; set; } = 15d;
    }

    private sealed class ImageOperationSettings
    {
        public bool ResizeEnabled { get; set; }
        public int ResizeWidth { get; set; } = DefaultResizeWidth;
        public int ResizeHeight { get; set; } = DefaultResizeHeight;
        public bool CropEnabled { get; set; }
        public int CropX { get; set; }
        public int CropY { get; set; }
        public int CropWidth { get; set; } = DefaultCropWidth;
        public int CropHeight { get; set; } = DefaultCropHeight;
        public bool RotateEnabled { get; set; }
        public double RotateDegrees { get; set; } = 15d;
        public int? SourceWidth { get; set; }
        public int? SourceHeight { get; set; }
    }

    private sealed class ImageDimensions
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    private sealed record DownloadItem(string FileName, Guid BatchId, Guid ImageId);
}

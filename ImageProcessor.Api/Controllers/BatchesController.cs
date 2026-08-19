using ImageProcessor.Api.Contracts.Http.Requests;
using ImageProcessor.Api.Contracts.Http.Responses;
using ImageProcessor.Application.Services;
using ImageProcessor.Application.Services.Models.BatchService;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BatchesController(IBatchService batchService, IObjectStorageService objectStorageService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateBatchResponse>> CreateBatch([FromBody] CreateBatchRequest request, CancellationToken cancellationToken)
    {
        if (request.ImagesMetadata.Count == 0)
        {
            return BadRequest(new { error = "At least one image is required." });
        }

        var batchId = Guid.NewGuid();

        var expectedImages = new List<RegisterExpectedImageCommand>(request.ImagesMetadata.Count);
        var presignedUploads = new List<PresignedUploadResponse>(request.ImagesMetadata.Count);

        foreach (var imageMetadata in request.ImagesMetadata)
        {
            var imageId = Guid.NewGuid();
            var presignedUpload = await objectStorageService.CreatePresignedUploadAsync(
                batchId,
                imageMetadata.FileName,
                imageMetadata.ContentType,
                cancellationToken);

            expectedImages.Add(new RegisterExpectedImageCommand
            {
                Id = imageId,
                S3Key = presignedUpload.S3Key,
                FileName = presignedUpload.FileName,
                ContentType = presignedUpload.ContentType
            });

            presignedUploads.Add(new PresignedUploadResponse
            {
                Id = imageId,
                S3Key = presignedUpload.S3Key,
                UploadUrl = presignedUpload.UploadUrl,
                ExpiresAtUtc = presignedUpload.ExpiresAtUtc,
                FileName = presignedUpload.FileName,
                ContentType = presignedUpload.ContentType
            });
        }

        var batch = await batchService.CreateBatchAsync(
            batchId,
            request.Operations,
            expectedImages,
            cancellationToken);

        var response = new CreateBatchResponse
        {
            Id = batch.Id,
            Status = batch.Status,
            CreatedAt = batch.CreatedAt,
            PresignedUploads = presignedUploads
        };

        return Created(response.Id.ToString(), response);
    }

    [HttpPost("{id}/start")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StartBatchResponse>> StartBatch(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await batchService.StartBatchAsync(id, cancellationToken);

            if (result is null)
            {
                return NotFound(new { error = "Batch not found." });
            }

            var response = new StartBatchResponse
            {
                Id = result.Id,
                Status = result.Status,
                CreatedAt = result.CreatedAt
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/status")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetBatchStatusResponse>> GetBatchStatus(Guid id, CancellationToken cancellationToken)
    {
        var result = await batchService.GetBatchAsync(id, cancellationToken);

        if (result is null)
        {
            return NotFound(new { error = "Batch not found." });
        }

        var response = new GetBatchStatusResponse
        {
            Id = result.Id,
            Status = result.Status,
            CreatedAt = result.CreatedAt
        };

        return Ok(response);
    }

    [HttpGet("{id}/images/{imageId}/download")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DownloadImage(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var image = await batchService.GetBatchImageAsync(id, imageId, cancellationToken);

        if (image is null)
        {
            return NotFound(new { error = "Image not found." });
        }

        if (image.Status != Domain.Entities.ImageStatus.Completed)
        {
            return Conflict(new { error = "Image is not yet processed." });
        }

        var metadata = await objectStorageService.GetObjectMetadataAsync(image.S3Key, cancellationToken);

        if (metadata is null)
        {
            return NotFound(new { error = "Image file not found in storage." });
        }

        var stream = await objectStorageService.GetObjectStreamAsync(image.S3Key, cancellationToken);

        return File(stream, metadata.ContentType, image.FileName);
    }
}

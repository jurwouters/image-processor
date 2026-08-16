using ImageProcessor.Api.Contracts.Http.Requests;
using ImageProcessor.Api.Contracts.Http.Responses;
using ImageProcessor.Application.Services;
using ImageProcessor.Application.Services.Models.BatchService;
using ImageProcessor.Application.Services.Models.Storage;
using ImageProcessor.Application.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BatchesController(IBatchService batchService, IUploadUrlService uploadService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateBatchResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBatch([FromBody] CreateBatchRequest request, CancellationToken cancellationToken)
    {
        if (request.ImagesMetadata.Count == 0)
        {
            return BadRequest(new { error = "At least one image is required." });
        }

        var batchId = Guid.NewGuid();

        var presignedUploads = new List<PresignedUploadResult>();
        foreach (var imageMetadata in request.ImagesMetadata)
        {
            var presignedUrl = await uploadService.CreatePresignedUploadAsync(
                batchId,
                imageMetadata.FileName,
                imageMetadata.ContentType,
                cancellationToken);

            presignedUploads.Add(presignedUrl);
        }

        var command = new CreateBatchCommand
        {
            Id = batchId,
            Operations = request.Operations,
            ExpectedImages = presignedUploads.Select(upload => new RegisterExpectedImageCommand
            {
                S3Key = upload.S3Key,
                FileName = upload.FileName,
                ContentType = upload.ContentType
            }).ToArray()
        };

        var batch = await batchService.CreateBatchAsync(command, cancellationToken);

        var response = new CreateBatchResponse
        {
            Id = batch.Id,
            Status = batch.Status,
            CreatedAt = batch.CreatedAt,
            PresignedUploads = presignedUploads.Select(upload => new PresignedUploadResponse
            {
                S3Key = upload.S3Key,
                UploadUrl = upload.UploadUrl,
                ExpiresAtUtc = upload.ExpiresAtUtc,
                FileName = upload.FileName,
                ContentType = upload.ContentType
            }).ToArray()
        };

        return Created(response.Id.ToString(), response);
    }

    [HttpPost("{id}/start")]
    [ProducesResponseType(typeof(StartBatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartBatch(Guid id, CancellationToken cancellationToken)
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

}

using System.Net.Http.Json;
using ImageProcessor.Web.Models;

namespace ImageProcessor.Web.Services;

public sealed class BatchApiClient(HttpClient httpClient)
{
    public async Task<CreateBatchResponseDto> CreateBatchAsync(CreateBatchRequestDto request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/batches", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CreateBatchResponseDto>(cancellationToken)
            ?? throw new InvalidOperationException("Create batch response was empty.");

        return payload;
    }

    public async Task UploadToPresignedUrlAsync(
        string uploadUrl,
        Stream contentStream,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
        {
            Content = new StreamContent(contentStream)
        };

        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        request.Content.Headers.ContentLength = contentLength;

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<StartBatchResponseDto> StartBatchAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync($"api/batches/{batchId}/start", content: null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<StartBatchResponseDto>(cancellationToken)
            ?? throw new InvalidOperationException("Start batch response was empty.");

        return payload;
    }

    public async Task<GetBatchStatusResponseDto> GetBatchStatusAsync(Guid batchId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/batches/{batchId}/status", cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GetBatchStatusResponseDto>(cancellationToken)
            ?? throw new InvalidOperationException("Get batch status response was empty.");

        return payload;
    }
}

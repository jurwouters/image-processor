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
        string contentType,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
        {
            Content = new StreamContent(contentStream)
        };

        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

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

    public static string BuildDownloadUrl(string uploadUrl)
    {
        var uri = new Uri(uploadUrl);
        var builder = new UriBuilder(uri)
        {
            Query = string.Empty
        };

        return builder.Uri.ToString();
    }
}

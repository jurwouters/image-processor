using ImageProcessor.Web.Components;
using ImageProcessor.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<BatchApiClient>((services, client) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Missing configuration: Api:BaseUrl");

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/download/{batchId:guid}/{imageId:guid}", async (
    Guid batchId,
    Guid imageId,
    BatchApiClient batchApiClient,
    CancellationToken cancellationToken) =>
{
    var response = await batchApiClient.DownloadImageAsync(batchId, imageId, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
        response.Dispose();
        return Results.NotFound();
    }

    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
    var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                   ?? response.Content.Headers.ContentDisposition?.FileName
                   ?? "download";

    return Results.Stream(
        async outputStream =>
        {
            using (response)
            {
                await response.Content.CopyToAsync(outputStream, cancellationToken);
            }
        },
        contentType,
        fileName);
});

app.Run();

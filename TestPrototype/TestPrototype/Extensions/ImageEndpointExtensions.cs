using TestPrototype.Services;
using TestPrototype.SharedUI.Models;

namespace TestPrototype.Extensions;

public static class ImageEndpointExtensions
{
    public static WebApplication MapImageEndpoints(this WebApplication app)
    {
        app.MapPost("/api/images/compose-post", async (
            PostImageComposeRequestDto request,
            SkiaSharpImageService imageService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.BackgroundImageUrl))
            {
                return Results.BadRequest(new { message = "Background image is required." });
            }

            if (request.ActivityData is null)
            {
                return Results.BadRequest(new { message = "Activity data is required." });
            }

            try
            {
                var result = await imageService.ComposePostImageAsync(request, cancellationToken);
                return Results.Ok(result);
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is FormatException ||
                ex is HttpRequestException ||
                ex is InvalidOperationException)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        return app;
    }
}

using SkiaSharp;
using TestPrototype.SharedUI.Models;

namespace TestPrototype.Services;

public class SkiaSharpImageService
{
    private const int OutputSize = 1080;
    private const int MaxSourceImageBytes = 1024 * 1024 * 5;
    private readonly IHttpClientFactory _httpClientFactory;

    public SkiaSharpImageService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PostImageComposeResultDto> ComposePostImageAsync(PostImageComposeRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.ActivityData is null)
        {
            throw new ArgumentException("Activity data is required.", nameof(request));
        }

        var sourceBytes = await LoadSourceImageBytesAsync(request.BackgroundImageUrl, cancellationToken);

        using var sourceBitmap = SKBitmap.Decode(sourceBytes)
            ?? throw new InvalidOperationException("Unable to decode the source image.");
        using var outputBitmap = new SKBitmap(OutputSize, OutputSize, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(outputBitmap);

        DrawBackground(canvas, sourceBitmap);
        DrawOverlay(canvas);
        DrawActivityData(canvas, request.ActivityData);

        using var image = SKImage.FromBitmap(outputBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 95);
        var base64 = Convert.ToBase64String(data.ToArray());

        return new PostImageComposeResultDto
        {
            ImageUrl = $"data:image/png;base64,{base64}"
        };
    }

    private async Task<byte[]> LoadSourceImageBytesAsync(string backgroundImageUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(backgroundImageUrl))
        {
            throw new ArgumentException("Background image is required.", nameof(backgroundImageUrl));
        }

        if (backgroundImageUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            var commaIndex = backgroundImageUrl.IndexOf(',');
            if (commaIndex < 0)
            {
                throw new InvalidOperationException("Invalid data URL image format.");
            }

            var base64 = backgroundImageUrl[(commaIndex + 1)..];
            var bytes = Convert.FromBase64String(base64);
            EnsureImageSize(bytes.Length);
            return bytes;
        }

        if (!Uri.TryCreate(backgroundImageUrl, UriKind.Absolute, out var imageUri) ||
            (imageUri.Scheme != Uri.UriSchemeHttp && imageUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Background image must be a data URL or HTTP/HTTPS URL.");
        }

        var httpClient = _httpClientFactory.CreateClient("ImageFetchClient");
        var bytesFromUrl = await httpClient.GetByteArrayAsync(imageUri, cancellationToken);
        EnsureImageSize(bytesFromUrl.Length);
        return bytesFromUrl;
    }

    private static void EnsureImageSize(int byteLength)
    {
        if (byteLength > MaxSourceImageBytes)
        {
            throw new InvalidOperationException("Source image exceeds the 5MB limit.");
        }
    }

    private static void DrawBackground(SKCanvas canvas, SKBitmap sourceBitmap)
    {
        var scale = Math.Max((float)OutputSize / sourceBitmap.Width, (float)OutputSize / sourceBitmap.Height);
        var scaledWidth = sourceBitmap.Width * scale;
        var scaledHeight = sourceBitmap.Height * scale;
        var left = (OutputSize - scaledWidth) / 2f;
        var top = (OutputSize - scaledHeight) / 2f;
        var destination = new SKRect(left, top, left + scaledWidth, top + scaledHeight);

        using var sourceImage = SKImage.FromBitmap(sourceBitmap);
        using var paint = new SKPaint
        {
            IsAntialias = true
        };

        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
        canvas.DrawImage(sourceImage, destination, sampling, paint);
    }

    private static void DrawOverlay(SKCanvas canvas)
    {
        using var dimPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 95),
            IsAntialias = true
        };
        canvas.DrawRect(0, 0, OutputSize, OutputSize, dimPaint);

        using var gradientPaint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, OutputSize * 0.55f),
                new SKPoint(0, OutputSize),
                new[] { new SKColor(0, 0, 0, 0), new SKColor(0, 0, 0, 210) },
                null,
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, OutputSize, OutputSize, gradientPaint);
    }

    private static void DrawActivityData(SKCanvas canvas, ActivityRecordDto activityData)
    {
        using var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
        using var labelFont = CreateFont(typeface, 36);
        using var valueFont = CreateFont(typeface, 150);
        using var unitFont = CreateFont(typeface, 46);
        using var metaFont = CreateFont(typeface, 54);
        using var labelPaint = CreateTextPaint(new SKColor(52, 211, 153));
        using var valuePaint = CreateTextPaint(SKColors.White);
        using var unitPaint = CreateTextPaint(new SKColor(255, 255, 255, 220));
        using var metaPaint = CreateTextPaint(SKColors.White);

        canvas.DrawText("DISTANCE", 72, 760, SKTextAlign.Left, labelFont, labelPaint);
        canvas.DrawText(activityData.Distance.ToString("0.0"), 72, 910, SKTextAlign.Left, valueFont, valuePaint);
        canvas.DrawText("KM", 430, 905, SKTextAlign.Left, unitFont, unitPaint);

        var duration = $"{(int)activityData.Duration.TotalHours}H {activityData.Duration.Minutes}M";
        canvas.DrawText("AVG HR", 690, 785, SKTextAlign.Left, labelFont, labelPaint);
        canvas.DrawText($"{activityData.HeartRate} BPM", 690, 850, SKTextAlign.Left, metaFont, metaPaint);
        canvas.DrawText("TIME", 690, 930, SKTextAlign.Left, labelFont, labelPaint);
        canvas.DrawText(duration, 690, 995, SKTextAlign.Left, metaFont, metaPaint);
    }

    private static SKFont CreateFont(SKTypeface typeface, float textSize)
    {
        return new SKFont(typeface, textSize)
        {
            Edging = SKFontEdging.Antialias
        };
    }

    private static SKPaint CreateTextPaint(SKColor color)
    {
        return new SKPaint
        {
            Color = color,
            IsAntialias = true
        };
    }
}

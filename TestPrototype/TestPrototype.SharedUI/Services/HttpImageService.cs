using System.Net.Http.Json;
using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services;

public class HttpImageService : IImageService
{
    private readonly HttpClient _httpClient;

    public HttpImageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PostImageComposeResultDto> ComposePostImageAsync(PostImageComposeRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/images/compose-post", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PostImageComposeResultDto>();
        return result ?? throw new InvalidOperationException("Image compose API returned an empty response.");
    }
}

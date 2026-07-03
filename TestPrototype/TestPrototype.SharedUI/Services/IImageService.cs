using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services;

public interface IImageService
{
    Task<PostImageComposeResultDto> ComposePostImageAsync(PostImageComposeRequestDto request);
}

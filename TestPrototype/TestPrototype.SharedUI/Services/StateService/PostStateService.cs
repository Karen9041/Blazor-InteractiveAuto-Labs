using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestPrototype.SharedUI.Enums;
using TestPrototype.SharedUI.Models;

namespace TestPrototype.SharedUI.Services.StateService;

public class PostStateService : IDisposable
{
    private readonly IPostApiService _postApiService;
    private readonly IAuthService _authService;
    private readonly IBrowserShareService _browserShareService;
    private readonly PersistentComponentState _applicationState;
    private readonly PersistingComponentStateSubscription _subscription;
    private readonly ConcurrentDictionary<string, string> _shareLinkCache = new();
    private bool _hasHydrated;
    private readonly HttpClient _httpClient;
    private readonly PostUploadService _postUploadService;

    public List<PostDto> Posts { get; private set; } = new();
    public UIState CurrentUIState { get; private set; } = UIState.Loading;

    public event Action? OnChange;

    public PostStateService(
        IPostApiService postApiService,
        IAuthService authService,
        IBrowserShareService browserShareService,
        PersistentComponentState applicationState,
        PostUploadService postUploadService)
    {
        _postApiService = postApiService;
        _authService = authService;
        _browserShareService = browserShareService;
        _applicationState = applicationState;
        _postUploadService = postUploadService;
        _subscription = _applicationState.RegisterOnPersisting(PersistData, RenderMode.InteractiveAuto);

        if (_applicationState.TryTakeFromJson<List<PostDto>>("feed_data", out var restored))
        {
            HydratePosts(restored);
            _hasHydrated = true;
        }

    }

    public async Task EnsureInitialTimelineLoadedAsync()
    {
        if (_hasHydrated)
        {
            _hasHydrated = false;
            return;
        }

        await LoadTimeLineAsync();
    }

    public async Task LoadTimeLineAsync()
    {
        try
        {
            CurrentUIState = UIState.Loading;
            NotifyStateChanged();

            Posts = await _postApiService.FetchTimelineAsync();
            CurrentUIState = Posts.Count == 0 ? UIState.Empty : UIState.Success;
        }
        catch (Exception ex)
        {
            CurrentUIState = UIState.Error;
            Console.WriteLine($"Timeline load failed: {ex.Message}");
        }
        finally
        {
            NotifyStateChanged();
        }
    }

    public async Task<PostDto?> LoadPostByIdAsync(string postId)
    {
        var post = await _postApiService.FetchPostByIdAsync(postId);
        return post;
    }

    public void HydratePosts(List<PostDto>? posts)
    {
        Posts = posts ?? new();
        CurrentUIState = Posts.Count == 0 ? UIState.Empty : UIState.Success;
        NotifyStateChanged();
    }

    public async Task PublishPostAsync(PostDto newPost)
    {
        // 調用獨立的 upload service 處裡繁重的上傳鏈
        var result = await _postUploadService.PublishCompletePostAsync(newPost);

        if (result != null && result.IsSuccess)
        {
            newPost.ImageUrl = result.FinalImageUrl;
            // Add to Posts list ...
            CurrentUIState = UIState.Success;
        }
        else
        {
            CurrentUIState = UIState.Error;
        }
        NotifyStateChanged();
    }

    public async Task ToggleLikeAsync(string postId)
    {
        if (!await _authService.RequireLoginAsync())
        {
            return;
        }

        var post = Posts.FirstOrDefault(p => p.Id == postId);
        if (post == null)
        {
            return;
        }

        var originalIsLiked = post.IsLikedByMe;
        var originalLikeCount = post.LikeCount;

        post.IsLikedByMe = !post.IsLikedByMe;
        post.LikeCount += post.IsLikedByMe ? 1 : -1;

        NotifyStateChanged();

        try
        {
            var apiSuccess = await _postApiService.ToggleLikeAsync(postId);

            if (!apiSuccess)
            {
                throw new HttpRequestException("Backend rejected the like action.");
            }
        }
        catch (Exception ex)
        {
            post.IsLikedByMe = originalIsLiked;
            post.LikeCount = originalLikeCount;
            NotifyStateChanged();

            Console.WriteLine($"Like failed, rollbacked. Error: {ex.Message}");
        }
    }

    public async Task PrepareShareLinkAsync(string postId)
    {
        try
        {
            var shareLink = await _postApiService.ExecuteShareAsync(postId);

            if (!string.IsNullOrWhiteSpace(shareLink))
            {
                _shareLinkCache[postId] = shareLink;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Share link preparation failed: {ex.Message}");
        }
    }

    public async Task CopyPostLinkAsync(string postId)
    {
        var shareLink = GetShareLink(postId);
        await _browserShareService.CopyToClipboardAsync(shareLink);
    }

    public async Task SharePostNativeAsync(string postId)
    {
        var post = Posts.FirstOrDefault(p => p.Id == postId);
        var shareLink = GetShareLink(postId);
        var shareTitle = post is null ? "Share post" : $"Share {post.AuthorName}'s post";
        const string shareText = "Check out this post.";

        var nativeShareResult = await _browserShareService.ShareAsync(shareTitle, shareText, shareLink);

        if (!nativeShareResult)
        {
            await CopyPostLinkAsync(postId);
        }
    }

    private string GetShareLink(string postId)
    {
        return _shareLinkCache.GetValueOrDefault(postId, $"/post/{Uri.EscapeDataString(postId)}");
    }

    private Task PersistData()
    {
        if (Posts.Any())
        {
            _applicationState.PersistAsJson("feed_data", Posts);
        }

        return Task.CompletedTask;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose()
    {
        _subscription.Dispose();
    }
}

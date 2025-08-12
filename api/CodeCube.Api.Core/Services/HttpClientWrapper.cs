using CodeCube.Api.Core.Interfaces;

namespace CodeCube.Api.Core.Services;

/// <summary>
/// Production HTTP client implementation
/// </summary>
public class HttpClientWrapper : IHttpClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed = false;

    public HttpClientWrapper()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "CodeCube-API-Crawler/1.0");
    }

    public HttpClientWrapper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetStringAsync(string url)
    {
        return await _httpClient.GetStringAsync(url);
    }

    public async Task<Stream> GetStreamAsync(string url)
    {
        return await _httpClient.GetStreamAsync(url);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
namespace CodeCube.Api.Core.Interfaces;

/// <summary>
/// Abstraction for HTTP client to enable unit testing with mock data
/// </summary>
public interface IHttpClient
{
    Task<string> GetStringAsync(string url);
    Task<Stream> GetStreamAsync(string url);
}
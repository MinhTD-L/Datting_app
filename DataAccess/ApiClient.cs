using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DataTransferObject;

public class ApiClient
{
    private readonly HttpClient _client;

    public ApiClient()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://litmatchclone-production.up.railway.app/");
    }

    private void AttachToken()
    {
        if (!string.IsNullOrEmpty(SessionManager.Token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", SessionManager.Token);
        }
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        AttachToken();

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(url, content);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Token expired");
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<TResponse>(
            responseJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    public async Task<string> PostRawAsync<TRequest>(string url, TRequest data)
    {
        AttachToken();

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(url, content);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Token expired");
        }

        return await response.Content.ReadAsStringAsync();
    }
    public async Task<TResponse> GetAsync<TResponse>(string url)
    {
        AttachToken();

        var response = await _client.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Token expired");
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<TResponse>(
            responseJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
    public async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest data)
    {
        AttachToken();

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync(url, content);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Token expired");
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<TResponse>(
            responseJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
    public async Task<TResponse?> DeleteAsync<TResponse>(string url)
    {
        AttachToken();

        var response = await _client.DeleteAsync(url);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Token expired");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
        }

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<TResponse>(
            responseJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
}


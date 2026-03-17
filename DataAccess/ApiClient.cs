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
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(ExtractErrorMessage(responseJson) ??
            $"Request failed with status code: {response.StatusCode}");
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(responseJson, options);
        }
        catch (JsonException)
        {
            var fixedJson = ExtractFirstJsonValue(responseJson);
            return JsonSerializer.Deserialize<TResponse>(fixedJson, options);
        }
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
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(ExtractErrorMessage(responseJson) ??
                                           $"Request failed with status code: {response.StatusCode}");
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(responseJson, options);
        }
        catch (JsonException)
        {
            var fixedJson = ExtractFirstJsonValue(responseJson);
            return JsonSerializer.Deserialize<TResponse>(fixedJson, options);
        }
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
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(ExtractErrorMessage(responseJson) ??
                                           $"Request failed with status code: {response.StatusCode}");
        }

        try
        {
            return JsonSerializer.Deserialize<TResponse>(responseJson, options);
        }
        catch (JsonException)
        {
            var fixedJson = ExtractFirstJsonValue(responseJson);
            return JsonSerializer.Deserialize<TResponse>(fixedJson, options);
        }
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
            var errorText = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(ExtractErrorMessage(errorText) ??
                                           $"Request failed with status code: {response.StatusCode}");
        }

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            return JsonSerializer.Deserialize<TResponse>(responseJson, options);
        }
        catch (JsonException)
        {
            var fixedJson = ExtractFirstJsonValue(responseJson);
            return JsonSerializer.Deserialize<TResponse>(fixedJson, options);
        }
    }

    public async Task<TResponse?> DeleteAsync<TRequest, TResponse>(string url, TRequest data)
    {
        AttachToken();

        var json = JsonSerializer.Serialize(data);
        using var req = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(req);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("Token expired");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(ExtractErrorMessage(errorText) ??
                                           $"Request failed with status code: {response.StatusCode}");
        }

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            return JsonSerializer.Deserialize<TResponse>(responseJson, options);
        }
        catch (JsonException)
        {
            var fixedJson = ExtractFirstJsonValue(responseJson);
            return JsonSerializer.Deserialize<TResponse>(fixedJson, options);
        }
    }

    private static string? ExtractErrorMessage(string responseText)
    {
        try
        {
            var json = ExtractFirstJsonValue(responseText);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("error", out var errEl))
                return errEl.GetString();
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static string ExtractFirstJsonValue(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input ?? string.Empty;

        var s = input.TrimStart();
        var start = s.IndexOfAny(new[] { '{', '[' });
        if (start < 0)
            return s.Trim();

        bool inString = false;
        bool escaped = false;
        int depth = 0;
        char open = s[start];
        char close = open == '{' ? '}' : ']';

        for (int i = start; i < s.Length; i++)
        {
            var ch = s[i];

            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (ch == '"')
                    inString = false;

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            if (ch == open) depth++;
            if (ch == close) depth--;

            if (depth == 0)
            {
                return s.Substring(start, i - start + 1).Trim();
            }
        }

        return s.Substring(start).Trim();
    }
}


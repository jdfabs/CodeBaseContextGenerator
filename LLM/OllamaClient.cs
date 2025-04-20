using System.Net.Http.Json;
using System.Text.Json;

namespace CodeBaseContextGenerator.LLM;


public class OllamaClient
{
    private readonly HttpClient _http = new();
    private const string ApiUrl = "http://localhost:11434/api/generate";

    public async Task<string> SendPromptAsync(string model, string prompt)
    {
        var response = await _http.PostAsJsonAsync(ApiUrl, new
        {
            model,
            prompt,
            stream = false
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return result?.Response ?? "ERROR";
    }

    public async Task StreamPromptAsync(string model, string prompt)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = JsonContent.Create(new { model, prompt, stream = true })
        };

        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        using var stream = await resp.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var json = JsonDocument.Parse(line);
            if (json.RootElement.TryGetProperty("response", out var token))
            {
                Console.Write(token.GetString());
                Console.Out.Flush();
            }
        }

        Console.WriteLine();
    }
}

public class OllamaResponse
{
    public string Response { get; set; } = string.Empty;
}
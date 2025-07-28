using System.Text;
using Newtonsoft.Json.Linq;

public static class TranslationHelper
{
    private static readonly HttpClient _client = new HttpClient();
    //private const string ApiKey = "5322d8f1c9msh9e5c97c3028733ap1d998ajsn13611fe01031"; // Secure via env var in prod
    private const string ApiKey = "ef20597bf2msh02caa84f5b7d609p11fc7ejsnb020b2e983d6";
    private const string Host = "rapid-translate-multi-traduction.p.rapidapi.com";
    private const string Endpoint = "https://rapid-translate-multi-traduction.p.rapidapi.com/t";

    public static async Task<string> TranslateAsync(string text, string from, string to)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Headers.Add("x-rapidapi-key", ApiKey);
        request.Headers.Add("x-rapidapi-host", Host);

        var body = new
        {
            from,
            to,
            q = text
        };

        string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var parsed = JArray.Parse(content);
        var txt = parsed[0]?.ToString() ?? text;
        return txt;

    }
}

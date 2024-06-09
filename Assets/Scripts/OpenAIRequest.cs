using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class OpenAIRequest : MonoBehaviour
{
    private const string RequestUri = "https://api.openai.com/v1/chat/completions";
    private const string ProxyAddress = "http://193.187.97.136:63392";
    private const string ProxyUserName = "4jkwhpTd";
    private const string ProxyPassword = "2G59HhrL";
    private const string Model = "gpt-3.5-turbo";
    private static HttpClient _client;

    public static async UniTask<string> MakeRequest(string prompt)
    {
        string apiKey = GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API key is missing or invalid.");
            return "API key is missing or invalid.";
        }

        string model = Model;

        var requestBody = new RequestBody
        {
            model = model,
            messages = new List<Message>
            {
                new() { role = "system", content = "Ты полезный чат-ассистент." },
                new() { role = "user", content = prompt }
            },
            max_tokens = 250
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var requestJson = JsonConvert.SerializeObject(requestBody);
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(RequestUri, requestContent);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<Response>(responseContent);
            var answer = jsonResponse.choices[0].message.content;
            return answer;
        }
        else
        {
            Debug.LogError("Error: " + response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.LogError("Error details: " + errorContent);
            return $"Error: {response.StatusCode}. Error details: {errorContent}";
        }
    }

    private void Start() => InitializeHttpClientWithProxy();

    private void InitializeHttpClientWithProxy()
    {
        var proxy = new WebProxy(ProxyAddress)
        {
            Credentials = new NetworkCredential(ProxyUserName, ProxyPassword)
        };

        var httpClientHandler = new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = true
        };

        _client = new HttpClient(httpClientHandler);
    }

    private static string GetApiKey()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var authPath = Path.Combine(homePath, ".openai", "auth.json");

        if (!File.Exists(authPath))
        {
            Debug.LogError("API key file not found: " + authPath);
            return null;
        }

        var authJson = File.ReadAllText(authPath);
        var authData = JsonConvert.DeserializeObject<Dictionary<string, string>>(authJson);

        if (authData != null && authData.TryGetValue("api_key", out var apiKey))
            return apiKey;

        Debug.LogError("API key not found in the file.");
        return null;
    }

    private class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    private class RequestBody
    {
        public string model { get; set; }
        public List<Message> messages { get; set; }
        public int max_tokens { get; set; }
    }

    private class Choice
    {
        public Message message { get; set; }
    }

    private class Response
    {
        public List<Choice> choices { get; set; }
    }
}
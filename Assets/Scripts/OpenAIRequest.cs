using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using System.Threading;

public class OpenAIRequest : MonoBehaviour
{
    [SerializeField] private string _model = "gpt-3.5-turbo";
    [SerializeField] private int _maxTokensCount = 200;

    private const string ConfigFileName = "config";
    private const string RequestUri = "https://api.openai.com/v1/chat/completions";

    private ConfigData _configData;
    private HttpClient _client;
    private CancellationTokenSource _cts;

    public async UniTask MakeStreamingRequest(string prompt, Action<string> onMessageReceived)
    {
        string apiKey = _configData.OpenAiApiKey;

        var requestBody = new RequestBody
        {
            Model = _model,
            Messages = new List<Message>
            {
                new() { Role = "system", Content = "Ты полезный чат-ассистент." },
                new() { Role = "user", Content = prompt }
            },
            MaxTokens = _maxTokensCount,
            Stream = true
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var requestJson = JsonConvert.SerializeObject(requestBody);
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, RequestUri)
        {
            Content = requestContent
        };

        var response = await _client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, _cts.Token);

        if (response.IsSuccessStatusCode)
        {
            var buffer = new byte[1024];
            var stream = await response.Content.ReadAsStreamAsync();
            string incompleteJson = string.Empty;
            using var reader = new System.IO.StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var length = await stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                if (length > 0)
                {
                    var chunk = Encoding.UTF8.GetString(buffer, 0, length);
                    incompleteJson += chunk;

                    int newLineIndex;
                    while ((newLineIndex = incompleteJson.IndexOf("\n")) != -1)
                    {
                        var line = incompleteJson[..newLineIndex].Trim();
                        incompleteJson = incompleteJson[(newLineIndex + 1)..];

                        if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

                        var json = line["data: ".Length..];
                        if (json == "[DONE]") break;

                        try
                        {
                            var jsonResponse = JsonConvert.DeserializeObject<StreamingResponse>(json);

                            foreach (var choice in jsonResponse.Choices)
                            {
                                if (choice.Delta != null && choice.Delta.Content != null)
                                {
                                    await UniTask.SwitchToMainThread();
                                    onMessageReceived?.Invoke(choice.Delta.Content);
                                    await UniTask.SwitchToThreadPool();
                                }
                            }
                        }
                        catch (JsonReaderException)
                        {
                            incompleteJson = json;
                        }
                        catch (JsonSerializationException ex)
                        {
                            Debug.LogWarning("Received unexpected JSON format: " + json);
                            Debug.LogWarning("Exception: " + ex.Message);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Error: " + response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.LogError("Error details: " + errorContent);
        }
    }

    private void Start()
    {
        LoadConfig();
        InitializeHttpClientWithProxy();
    }

    private void OnDestroy() => DisposeHttpClient();

    private void OnApplicationQuit() => DisposeHttpClient();

    private void DisposeHttpClient()
    {
        _cts?.Cancel();
        _client?.Dispose();
        _client = null;
    }

    private void LoadConfig()
    {
        TextAsset configTextAsset = Resources.Load<TextAsset>(ConfigFileName);

        if (configTextAsset != null)
        {
            string json = configTextAsset.text;
            _configData = JsonConvert.DeserializeObject<ConfigData>(json);
        }
        else
        {
            Debug.LogError("Config file not found in Resources.");
        }
    }

    private void InitializeHttpClientWithProxy()
    {
        var proxyParts = _configData.ProxyHttp.Split(":");
        var proxy = new WebProxy($"http://{proxyParts[0]}:{proxyParts[1]}")
        {
            Credentials = new NetworkCredential(proxyParts[2], proxyParts[3])
        };

        var httpClientHandler = new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = true
        };

        _client = new HttpClient(httpClientHandler);
        _cts = new CancellationTokenSource();
    }

    private class ConfigData
    {
        [JsonProperty("OPENAI_API_KEY")] public string OpenAiApiKey { get; set; }
        [JsonProperty("PROXY_HTTP")] public string ProxyHttp { get; set; }
    }

    private class Message
    {
        [JsonProperty("role")] public string Role { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
    }

    private class RequestBody
    {
        [JsonProperty("model")] public string Model { get; set; }
        [JsonProperty("messages")] public List<Message> Messages { get; set; }
        [JsonProperty("max_tokens")] public int MaxTokens { get; set; }
        [JsonProperty("stream")] public bool Stream { get; set; }
    }

    private class Delta
    {
        [JsonProperty("content")] public string Content { get; set; }
    }

    private class Choice
    {
        [JsonProperty("delta")] public Delta Delta { get; set; }
    }

    private class StreamingResponse
    {
        [JsonProperty("choices")] public List<Choice> Choices { get; set; }
    }
}

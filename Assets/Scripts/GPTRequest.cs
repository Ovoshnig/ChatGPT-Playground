/*using Cysharp.Threading.Tasks;
using OpenAI.Chat;
using System;
using System.Net;
using System.Net.Http;
using System.ClientModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI;

public class OpenAIChat : MonoBehaviour
{
    [SerializeField] private TMP_InputField _userInputField;
    [SerializeField] private Button _sendButton;
    [SerializeField] private TMP_Text _responseText;

    private const string ApiKey = "sk-proj-KjGnvcxbf7xIJlH2jnblT3BlbkFJnj6k1HeLs3DOFhNp9N8U";
    private const string ProxyString = "193.187.97.136:63392:4jkwhpTd:2G59HhrL";

    private void Start()
    {
        _sendButton.onClick.AddListener(OnSendButtonClicked);
    }

    private void OnSendButtonClicked()
    {
        string userInput = _userInputField.text;
        if (!string.IsNullOrEmpty(userInput))
        {
            SendRequest(userInput).Forget();
        }
    }

    private async UniTask SendRequest(string input)
    {
        string[] proxyParts = ProxyString.Split(":");
        var proxy = new WebProxy
        {
            Address = new Uri($"http://{proxyParts[0]}:{proxyParts[1]}"),
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false,

            Credentials = new NetworkCredential(
                userName: proxyParts[2],
                password: proxyParts[3])
        };

        WebRequest.DefaultWebProxy = proxy;

        var chatClient = new ChatClient("gpt-3.5-turbo", new ApiKeyCredential(ApiKey));

        var asyncChatUpdates = chatClient.CompleteChatStreamingAsync(
            new ChatMessage[]
            {
                new UserChatMessage("Напиши перцептрон на Python с Keras")
            });

        Console.WriteLine($"[ASSISTANT]:");
        await foreach (var chatUpdate in asyncChatUpdates)
        {
            foreach (var contentPart in chatUpdate.ContentUpdate)
            {
                _responseText.text += contentPart.Text;
            }
        }
    }
}
*/
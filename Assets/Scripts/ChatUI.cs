using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OpenAIRequest))]
public class ChatUI : MonoBehaviour
{
    [SerializeField] private float _printDelay;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _sendButton;
    [SerializeField] private TMP_Text _responseText;

    private OpenAIRequest _openAIRequest;

    private void Awake() => _openAIRequest = GetComponent<OpenAIRequest>();

    private void Start() => _sendButton.onClick.AddListener(() => OnDoneButtonClick().Forget());

    private async UniTask OnDoneButtonClick()
    {
        if (string.IsNullOrEmpty(_inputField.text))
        {
            Debug.LogError("Нет запроса");
            return;
        }

        _responseText.text = "";
        await _openAIRequest.MakeStreamingRequest(_inputField.text, message => _responseText.text += message);
    }
}
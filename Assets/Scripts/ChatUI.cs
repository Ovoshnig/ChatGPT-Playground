using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private float _printDelay;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TMP_Text _answerText;
    [SerializeField] private Button _doneButton;

    private void Start() => _doneButton.onClick.AddListener(() => OnDoneButtonClick().Forget());

    private async UniTask OnDoneButtonClick()
    {
        if (string.IsNullOrEmpty(_inputField.text))
        {
            Debug.LogError("Нет запроса");
            return;
        }

        _answerText.text = "";
        string answer = await OpenAIRequest.MakeRequest(_inputField.text);

        foreach (char symbol in answer)
        {
            _answerText.text += symbol;
            await UniTask.WaitForSeconds(_printDelay);
        }
    }
}
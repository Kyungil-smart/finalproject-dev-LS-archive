using Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultArea : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _resultText;
    private Button _button;

    public void SetUpText(string text)
    {
        _resultText.text = text;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_button != null)
        {
            _button.onClick.AddListener(OnClickResultButton);
        }
    }

    private void OnClickResultButton()
    {
        SceneManager.LoadScene(Define.SCENE_LOBBY);
    }
}
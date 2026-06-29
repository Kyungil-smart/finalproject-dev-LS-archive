using Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultArea : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private Button _button;

    public void SetUpText(string text)
    {
        _resultText.text = text;
    }

    private void Awake()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(OnClickResultButton);
        }
    }

    private void OnClickResultButton()
    {
        Debug.Log("[Result Button] : On Click");
        SceneManager.LoadScene(Define.SCENE_LOBBY);
    }
}
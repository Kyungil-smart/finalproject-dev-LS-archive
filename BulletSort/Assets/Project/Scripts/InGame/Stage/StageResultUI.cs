using Core;
using UnityEngine;


namespace Ingame
{
    public class StageResultUI : MonoBehaviour
    {
        [SerializeField] private GameObject _uiRoot;
        [SerializeField] private ResultArea _resultArea;

        private string _winText = "승리";
        private string _defeatText = "패배";

        private void Start()
        {
            StageManager.Instance.OnStageWin += WinHandler;
            StageManager.Instance.OnStageDefeat += DefeatHandler;

            _uiRoot.SetActive(false);
        }

        private void WinHandler()
        {
            _uiRoot.SetActive(true);
            _resultArea.SetUpText(_winText);
        }

        private void DefeatHandler()
        {
            _uiRoot.SetActive(true);
            _resultArea.SetUpText(_defeatText);
        }
    }
}
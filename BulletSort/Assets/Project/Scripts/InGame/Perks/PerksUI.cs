using Core;
using TMPro;
using UnityEngine;

namespace Ingame.Perks
{
    public class PerksUI : MonoBehaviour
    {
        [SerializeField] private GameObject _uiRoot;
        [SerializeField] private PerksUIButton[] _perkButtons;

        [SerializeField] private TextMeshProUGUI _remainSelectNumText;

        private void Start()
        {
            PerksManager.Instance.OnPerksRolled += OpenWindow;
            PerksManager.Instance.OnPerkSelected += UpdateRemainSelectNum;

            _uiRoot.SetActive(false);
        }

        private void OnEnable()
        {
            UpdateRemainSelectNumText();
        }

        private void OpenWindow(int[] perksIDs)
        {
            _uiRoot.SetActive(true);

            for (int iCnt = 0; iCnt < _perkButtons.Length; iCnt++)
            {
                if (iCnt < perksIDs.Length)
                {
                    _perkButtons[iCnt].gameObject.SetActive(true);
                    _perkButtons[iCnt].SetUp(perksIDs[iCnt]);
                }
                else
                {
                    _perkButtons[iCnt].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateRemainSelectNum()
        {
            if (PerksManager.Instance.RemainSelectNum == 0)
            {
                CloseWindow();
                return;
            }

            UpdateRemainSelectNumText();
        }

        private void CloseWindow()
        {
            _uiRoot.SetActive(false);
        }

        private void UpdateRemainSelectNumText()
        {
            _remainSelectNumText.text = $"특전 선택하기 (남은 횟수 : {PerksManager.Instance.RemainSelectNum})";
        }
    }
}
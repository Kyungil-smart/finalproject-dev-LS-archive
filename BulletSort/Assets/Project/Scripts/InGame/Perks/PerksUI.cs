using Core;
using UnityEngine;

namespace Ingame.Perks
{
    public class PerksUI : MonoBehaviour
    {
        [SerializeField] private GameObject _uiRoot;
        [SerializeField] private PerksUIButton[] _perkButtons;

        private void Start()
        {
            PerksManager.Instance.OnPerksRolled += OpenWindow;
            PerksManager.Instance.OnPerkSelected += CloseWindow;

            _uiRoot.SetActive(false);
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

        private void CloseWindow()
        {
            _uiRoot.SetActive(false);
        }
    }
}
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat;

namespace Ingame.Perks
{
    public class PerksUI : MonoBehaviour
    {
        [SerializeField] private GameObject _uiRoot;
        [SerializeField] private PerksUIButton[] _perkButtons;

        [SerializeField] private TextMeshProUGUI _remainSelectNumText;

        [SerializeField] private LocalizedString _perkNumString;

        private void Awake()
        {
            if (PerksManager.Instance == null)
            {
                return;
            }

            PerksManager.Instance.OnPerksRolled += OpenWindow;
            PerksManager.Instance.OnPerkSelected += UpdateRemainSelectNum;
            PerksManager.Instance.OnPerkPhaseEnded += CloseWindow;
        }

        private void OnDestroy()
        {
            if (PerksManager.Instance == null)
            {
                return;
            }

            PerksManager.Instance.OnPerksRolled -= OpenWindow;
            PerksManager.Instance.OnPerkSelected -= UpdateRemainSelectNum;
            PerksManager.Instance.OnPerkPhaseEnded -= CloseWindow;
        }

        private void Start()
        {
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
            UpdateRemainSelectNumText();
        }

        private void CloseWindow()
        {
            _uiRoot.SetActive(false);
        }

        private void UpdateRemainSelectNumText()
        {
            var runtimeDataWrapper = new
            {
                RuntimeData = new
                {
                    PerkChoiceCompletedCount = PerksManager.Instance.RemainSelectNum,
                    PerkChoiceTotalCount = PerksManager.Instance.TotalSelectNum
                }
            };

            _remainSelectNumText.text = Smart.Format(_perkNumString.GetLocalizedString(), runtimeDataWrapper);
        }
    }
}
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.UI;

namespace Ingame.Perks
{
    public class PerksUIRerollArea : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rerollRemainNumText;
        [SerializeField] private LocalizedString _rerollRemainText;
        [SerializeField] Button _rerollButton;

        private void Awake()
        {
            if (_rerollButton != null)
            {
                _rerollButton.onClick.AddListener(OnClickRerollButton);
            }
        }

        private void Start()
        {
            PerksManager.Instance.OnRerolled += UpdateRemainRerollNum;
        }

        private void OnEnable()
        {
            _rerollButton.interactable = true;

            var runtimeDataWrapper = new
            {
                RuntimeData = new
                {
                    RerollCount = PerksManager.Instance.RemainRerollNum
                }
            };

            _rerollRemainNumText.text = Smart.Format(_rerollRemainText.GetLocalizedString(), runtimeDataWrapper);
        }

        public void UpdateRemainRerollNum()
        {
            int remainRerollNum = PerksManager.Instance.RemainRerollNum;

            if (remainRerollNum == 0)
            {
                _rerollButton.interactable = false;
            }

            var runtimeDataWrapper = new
            {
                RuntimeData = new
                {
                    RerollCount = PerksManager.Instance.RemainRerollNum
                }
            };

            _rerollRemainNumText.text = Smart.Format(_rerollRemainText.GetLocalizedString(), runtimeDataWrapper);
        }

        private void OnClickRerollButton()
        {
            //Debug.Log($"[Perk Reroll Button] : On Click");
            PerksManager.Instance.Reroll();
        }
    }
}
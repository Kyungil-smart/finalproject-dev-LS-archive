using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
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
            _rerollRemainText.Arguments = new object[] { PerksManager.Instance.RemainRerollNum };
            _rerollRemainNumText.text = _rerollRemainText.GetLocalizedString();
        }

        public void UpdateRemainRerollNum()
        {
            int remainRerollNum = PerksManager.Instance.RemainRerollNum;

            if (remainRerollNum == 0)
            {
                _rerollButton.interactable = false;
            }

            _rerollRemainText.Arguments = new object[] { remainRerollNum };
            _rerollRemainNumText.text = _rerollRemainText.GetLocalizedString();
        }

        private void OnClickRerollButton()
        {
            //Debug.Log($"[Perk Reroll Button] : On Click");
            PerksManager.Instance.Reroll();
        }
    }
}
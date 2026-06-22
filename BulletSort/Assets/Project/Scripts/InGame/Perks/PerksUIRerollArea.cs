using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ingame.Perks
{
    public class PerksUIRerollArea : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rerollRemainNumText;
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
            _rerollRemainNumText.text = $"남은 횟수 : {PerksManager.Instance.RemainRerollNum}";
        }

        public void UpdateRemainRerollNum()
        {
            int remainRerollNum = PerksManager.Instance.RemainRerollNum;

            if (remainRerollNum == 0)
            {
                _rerollButton.interactable = false;
            }

            _rerollRemainNumText.text = $"남은 횟수 : {remainRerollNum}";
        }

        private void OnClickRerollButton()
        {
            Debug.Log($"[Perk Button] : On Click");
            PerksManager.Instance.Reroll();
        }
    }
}
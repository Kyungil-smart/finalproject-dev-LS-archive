using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ingame.Perks
{
    public class PerksUIButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _perkName;
        [SerializeField] private TextMeshProUGUI _perkLevel;
        [SerializeField] private TextMeshProUGUI _perkDesc;
        [SerializeField] private TextMeshProUGUI _perkEffect;
        [SerializeField] private int _index;

        private Image _panelImage;
        [SerializeField] private Sprite _normalPanel;
        [SerializeField] private Sprite _rarePanel;
        [SerializeField] private Sprite _uniquePanel;
        [SerializeField] private Sprite _legendaryPanel;

        string _name;
        string _desc;
        int _curLevel;
        string _effectText;
        string _targetText;

        private Button _button;
        private int _perkID;

        private void Awake()
        {
            _panelImage = GetComponent<Image>();
            _button = GetComponent<Button>();

            if (_button != null)
            {
                _button.onClick.AddListener(OnClickPerkButton);
            }
        }

        public void SetUp(int perkID)
        {
            PerkData perk = DataManager.Instance.GetData<PerkData>(perkID);

            _name = perk.PerkName;
            _desc = perk.PerkDesc;
            _curLevel = perk.CurLevel;

            _effectText = perk.PerkDesc;    // effect의 desc로 수정 필요. 임시값.

            _targetText = perk.PerkTargetText;

            switch (perk.PerkRarityType)
            {
                case 91:
                    _panelImage.sprite = _normalPanel;
                    break;
                case 92:
                    _panelImage.sprite = _rarePanel;
                    break;
                case 93:
                    _panelImage.sprite = _uniquePanel;
                    break;
                case 94:
                    _panelImage.sprite = _legendaryPanel;
                    break;
                default:
                    Debug.LogError($"Invalid Value Perk Rarity Type");
                    break;
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_perkName != null)
            {
                _perkName.text = _name;
            }

            if (_perkLevel != null)
            {
                _perkLevel.text = $"Lv {_curLevel} -> {_curLevel + 1}";
            }

            if (_perkDesc != null)
            {
                _perkDesc.text = _desc;
            }

            if (_perkEffect != null)
            {
                _perkEffect.text = _effectText;
            }
        }

        private void OnClickPerkButton()
        {
            Debug.Log($"[Perk Button] : On Click");
            PerksManager.Instance.SelectPerk(_index);
        }
    }
}
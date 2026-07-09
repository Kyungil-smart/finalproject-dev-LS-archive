using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Ingame.Perks
{
    public class PerksUIButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _perkName;
        [SerializeField] private TextMeshProUGUI _perkDesc;
        [SerializeField] private TextMeshProUGUI _perkTarget;
        [SerializeField] private TextMeshProUGUI _perkRarity;
        [SerializeField] private int _index;

        [SerializeField] private LocalizedString _perkNewNameLS;
        [SerializeField] private LocalizedString _perkLvUpNameLS;
        [SerializeField] private LocalizedString _perkDescLS;
        [SerializeField] private LocalizedString _perkTargetLS;
        [SerializeField] private LocalizedString _perkRarityLS; //Todo

        [SerializeField] Image _panelImage;
        [SerializeField] Image _perkIconBackground;
        [SerializeField] Image _perkIcon;

        [SerializeField] private Sprite _normalPanel;
        [SerializeField] private Sprite _rarePanel;
        [SerializeField] private Sprite _uniquePanel;
        [SerializeField] private Sprite _legendaryPanel;

        [SerializeField] private Sprite _normalIcon;
        [SerializeField] private Sprite _rareIcon;
        [SerializeField] private Sprite _uniqueIcon;
        [SerializeField] private Sprite _legendaryIcon;

        string _name;
        string _desc;
        int _curLevel;
        string _effectText;
        string _targetText;

        private Button _button;
        private int _perkID;

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_button != null)
            {
                _button.onClick.AddListener(OnClickPerkButton);
            }
        }

        public void SetUp(int perkID)
        {
            PerkData perk = DataManager.Instance.GetData<PerkData>(perkID);

            if (perk.CurLevel > 1)
            {
                _perkLvUpNameLS.Arguments = new object[] { perk.PerkName, perk.CurLevel, perk.CurLevel + 1 };
                _name = _perkLvUpNameLS.GetLocalizedString();
            }
            else
            {
                _perkNewNameLS.Arguments = new object[] { perk.PerkName };
                _name = _perkNewNameLS.GetLocalizedString();
            }

            _perkDescLS.Arguments = new object[] { perk.PerkDesc };
            _desc = _perkDescLS.GetLocalizedString();

            _perkTargetLS.Arguments = new object[] {
                LocalizationSettings.StringDatabase.GetLocalizedString(
                    "LocalizationTable",
                    perk.PerkTargetText
                )
            };
            _targetText = _perkTargetLS.GetLocalizedString();

            switch (perk.PerkRarityType)
            {
                case 91:
                    _panelImage.sprite = _normalPanel;
                    _perkIcon.sprite = _normalIcon;
                    break;
                case 92:
                    _panelImage.sprite = _rarePanel;
                    _perkIcon.sprite = _rareIcon;
                    break;
                case 93:
                    _panelImage.sprite = _uniquePanel;
                    _perkIcon.sprite = _uniqueIcon;
                    break;
                case 94:
                    _panelImage.sprite = _legendaryPanel;
                    _perkIcon.sprite = _legendaryIcon;
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

            if (_perkDesc != null)
            {
                _perkDesc.text = _desc;
            }

            if (_perkTarget != null)
            {
                _perkTarget.text = _effectText;
            }
        }

        private void OnClickPerkButton()
        {
            Debug.Log($"[Perk Button] : On Click");
            PerksManager.Instance.SelectPerk(_index);
        }
    }
}
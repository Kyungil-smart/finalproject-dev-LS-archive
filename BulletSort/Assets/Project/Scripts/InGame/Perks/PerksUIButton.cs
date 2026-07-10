using Core;
using System.Collections.Generic;
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
        [SerializeField] Image _perkSkillIcon;
        [SerializeField] Image _perkTargetIcon;

        [SerializeField] private Sprite[] _panels;
        [SerializeField] private Sprite[] _icons;

        [SerializeField] private Sprite[] _targetIcons;

        struct IconSpritePair
        {
            public string _name;
            public Sprite _sprite;
        }


        [SerializeField] private IconSpritePair[] _skillIconList;
        private Dictionary<string, Sprite> _skillIcons;

        string _name;
        string _desc;
        string _targetText;
        string _rarityText;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_button != null)
            {
                _button.onClick.AddListener(OnClickPerkButton);
            }
        }

        private void OnEnable()
        {
            _skillIcons = new Dictionary<string, Sprite>();

            foreach (var pair in _skillIconList)
            {
                _skillIcons[pair._name] = pair._sprite;
            }
        }

        public void SetUp(int perkID)
        {
            PerkData perk = DataManager.Instance.GetData<PerkData>(perkID);
            RarityData rarity = DataManager.Instance.GetData<RarityData>(perk.PerkRarityType);

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

            _perkRarityLS.Arguments = new object[] {
                    LocalizationSettings.StringDatabase.GetLocalizedString(
                    "LocalizationTable",
                    rarity.name
                )
            };
            _rarityText = _perkRarityLS.GetLocalizedString();

            _panelImage.sprite = _panels[perk.PerkRarityType - 91];
            _perkIconBackground.sprite = _icons[perk.PerkRarityType - 91];

            _perkTargetIcon.sprite = _targetIcons[perk.PerkTarget - 1];

            _perkSkillIcon.sprite = _skillIcons[perk.IconResourceKey];

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
                _perkTarget.text = _targetText;
            }

            if (_perkRarity != null)
            {
                _perkRarity.text = _rarityText;
            }
        }

        private void OnClickPerkButton()
        {
            Debug.Log($"[Perk Button] : On Click");
            PerksManager.Instance.SelectPerk(_index);
        }
    }
}
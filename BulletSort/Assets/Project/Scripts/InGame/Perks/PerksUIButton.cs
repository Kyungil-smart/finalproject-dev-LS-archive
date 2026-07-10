using Core;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat;
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

        [Serializable]
        public struct IconSpritePair
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
                string perkName = LocalizationSettings.StringDatabase.GetLocalizedString(
                    "LocalizationTable",
                    perk.PerkName
                );

                var runtimeDataWrapper = new
                {
                    PerkData = new
                    {
                        PerkName = perkName
                    },

                    RuntimeData = new
                    {
                        PerkLv = perk.CurLevel,
                        PerkLvNext = perk.CurLevel + 1
                    }

                };

                _name = Smart.Format(_perkLvUpNameLS.GetLocalizedString().Replace("RuntimeData.PerkLv+1", "RuntimeData.PerkLvNext"), runtimeDataWrapper);
            }
            else
            {
                string perkName = LocalizationSettings.StringDatabase.GetLocalizedString(
                    "LocalizationTable",
                    perk.PerkName
                );

                var runtimeDataWrapper = new
                {
                    PerkData = new
                    {
                        PerkName = perkName
                    },
                };

                _name = Smart.Format(_perkNewNameLS.GetLocalizedString(), runtimeDataWrapper);
            }

            {
                string perkDesc = LocalizationSettings.StringDatabase.GetLocalizedString(
                    "LocalizationTable",
                    perk.PerkDesc
                );
                var runtimeDataWrapper = new
                {
                    PerkData = new
                    {
                        PerkDesc = perkDesc
                    },
                };

                perkDesc = Smart.Format(_perkDescLS.GetLocalizedString(), runtimeDataWrapper);

                EffectData effect = DataManager.Instance.GetData<EffectData>(perk.EffectID);

                var effectDataWrapper = new
                {
                    EffectData = new
                    {
                        ATK = effect.ATK,
                        ATKSpeed = effect.ATKSpeed + 1,
                        MaxProj = effect.MaxProj,
                        ProjPiercing = effect.ProjPiercing,
                        ShotProjCount = effect.ShotProjCount,
                        Duration = effect.Duration,
                        BuffType = new
                        {
                            BoomArea = (int)(effect.BuffValue * 100),
                            Heal = (int)(effect.BuffValue * 100),
                            HP = (int)(effect.BuffValue * 100)
                        },
                        BuffValue = effect.BuffValue,
                    }
                };

                _desc = Smart.Format(perkDesc, effectDataWrapper);
            }

            {
                if (perk.PerkTargetText == "0")
                {
                    _targetText = "";
                }
                else
                {
                    string perkTarget = LocalizationSettings.StringDatabase.GetLocalizedString(
                        "LocalizationTable",
                        perk.PerkTargetText
                    );
                    var runtimeDataWrapper = new
                    {
                        PerkData = new
                        {
                            PerkTargetText = perkTarget
                        },
                    };
                    _targetText = Smart.Format(_perkTargetLS.GetLocalizedString(), runtimeDataWrapper);
                }
            }

            {
                string rarityName = LocalizationSettings.StringDatabase.GetLocalizedString(
                        "LocalizationTable",
                        rarity.PerkRarityName
                    );

                var runtimeDataWrapper = new
                {
                    RarityData = new
                    {
                        PerkRarityName = rarityName
                    },
                };
                _rarityText = Smart.Format(_perkRarityLS.GetLocalizedString(), runtimeDataWrapper);
            }

            _panelImage.sprite = _panels[perk.PerkRarityType - 91];
            _perkIconBackground.sprite = _icons[perk.PerkRarityType - 91];

            if (perk.PerkTargetText != "0")
            {
                _perkTargetIcon.sprite = _targetIcons[perk.PerkTarget - 1];
                _perkTargetIcon.gameObject.SetActive(true);
            }
            else
            {
                _perkTargetIcon.gameObject.SetActive(false);
            }

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
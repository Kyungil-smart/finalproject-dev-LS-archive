
using Core;
using System;
using System.Collections.Generic;
using Towers.Struct.TowerInfo;
using UnityEngine;

namespace Ingame.Perks
{
    // Effect ID 순서가 TowerType을 따르지 않음.
    public enum TowerGroupType
    {
        Basic = 1,
        Shotgun = 2,
        SR = 3,
        Mortar = 4,
        Boomer = 5,
        Buffer = 6,
        Global = 9
    }

    public class EffectBonusValue
    {
        public TowerGroupType type;
        public int BonusATK = 0;
        public float BonusATKSpeed = 0;
        public int BonusShotProjCount = 0;
        public int BonusMaxAmmo = 0;
        public float ProjSize = 0;
        public int BonusProjPiercing = 0;
        public float BonusBuffValue = 0;
        // ...
    }
    class EffectManager : MonoBehaviour
    {
        public event Action<EffectBonusValue> OnEffectApply;

        private Dictionary<TowerGroupType, EffectBonusValue> _groupEffectBonus = new Dictionary<TowerGroupType, EffectBonusValue>();
        public Dictionary<TowerGroupType, EffectBonusValue> GroupEffectBonus { get { return _groupEffectBonus; } }

        private IReadOnlyDictionary<int, EffectData> _effectDict;
        private void Awake()
        {
            foreach (TowerGroupType type in System.Enum.GetValues(typeof(TowerGroupType)))
            {
                _groupEffectBonus[type] = new EffectBonusValue();
            }
        }

        private void Start()
        {
            _effectDict = DataManager.Instance.GetTable<EffectData>();
            PerksManager.Instance.BindEffectManager(this);
        }

        public EffectBonusValue GetBonusValueByTowerInfo(STowerInfo info)
        {
            return _groupEffectBonus[(TowerGroupType)info.TowerType];
        }

        public void ApplyEffect(int effectID)
        {
            EffectData data = _effectDict[effectID];

            TowerGroupType type = GetTowerType(data.EffectID);

            var bonus = _groupEffectBonus[type];

            bonus.type = type;
            bonus.BonusATK += data.ATK;
            bonus.BonusATKSpeed += data.ATKSpeed;
            bonus.BonusShotProjCount += data.ShotProjCount;
            bonus.BonusMaxAmmo += data.MaxProj;
            bonus.BonusProjPiercing += data.ProjPiercing;
            bonus.BonusBuffValue += data.BuffValue;

            OnEffectApply(bonus);
        }

        private TowerGroupType GetTowerType(int id)
        {
            if (9001 <= id && id <= 9007)
            {
                return TowerGroupType.Basic;
            }
            else if (9008 <= id && id <= 9012)
            {
                return TowerGroupType.Shotgun;
            }
            else if (9013 <= id && id <= 9016)
            {
                return TowerGroupType.Mortar;
            }
            else if (9017 <= id && id <= 9021)
            {
                return TowerGroupType.SR;
            }
            else if (9022 <= id && id <= 9025)
            {
                return TowerGroupType.Buffer;
            }
            else if (9026 <= id && id <= 9030)
            {
                return TowerGroupType.Boomer;
            }
            else
            {
                return TowerGroupType.Global;
            }
        }
    }
}
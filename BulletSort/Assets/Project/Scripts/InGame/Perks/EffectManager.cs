
using System.Collections.Generic;
using UnityEngine;

namespace Ingame.Perks
{
    public enum TowerGroupType
    {
        Basic = 0,
        Shotgun = 1,
        Mortar = 2,
        SR = 3,
        Buffer = 4,
        Boomer = 5,
        Global = 6
    }

    public class EffectBonusValue
    {
        public float BonusATK = 0;
        public float BonusATKSpeed = 0;
        public float BonusShotProjCount = 0;
        public float BonusMaxAmmo = 0;
        public float BonusProjPiercing = 0;
        public float BonusBuffValue = 0;
        // ...
    }
    class EffectManager : MonoBehaviour
    {
        private Dictionary<TowerGroupType, EffectBonusValue> _groupEffectBonus = new Dictionary<TowerGroupType, EffectBonusValue>();

        private void Awake()
        {
            foreach (TowerGroupType type in System.Enum.GetValues(typeof(TowerGroupType)))
            {
                _groupEffectBonus[type] = new EffectBonusValue();
            }
        }

        public void ApplyEffect(EffectData data)
        {
            TowerGroupType type = GetTowerType(data.EffectID);

            var bonus = _groupEffectBonus[type];

            bonus.BonusATK += data.ATK;
            bonus.BonusATKSpeed += data.ATKSpeed;
            bonus.BonusShotProjCount += data.ShotProjCount;
            bonus.BonusMaxAmmo += data.MaxProj;
            bonus.BonusProjPiercing += data.ProjPiercing;
            bonus.BonusBuffValue += data.BuffValue;
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
using Core;
using UnityEngine;

namespace Ingame.Perks
{
    public class PerksUIButton : MonoBehaviour
    {
        string _name;
        string _desc;
        int _curLevel;
        string _targetText;

        public void SetUp(int perkID)
        {
            PerkData perk = DataManager.Instance.GetData<PerkData>(perkID);

            // ...

            _name = perk.PerkName;
            _desc = perk.PerkDesc;
            //_curLevel = perk.     // partial class로 cur level 저장할 예정.

            _targetText = perk.PerkTargetText;
        }
    }
}
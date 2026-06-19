using Core;
using UnityEngine;

namespace Ingame.Perks
{
    public class PerksUIButton : MonoBehaviour
    {
        public void SetUp(int perkID)
        {
            PerkData perk = DataManager.Instance.GetData<PerkData>(perkID);

            // ...
        }
    }
}
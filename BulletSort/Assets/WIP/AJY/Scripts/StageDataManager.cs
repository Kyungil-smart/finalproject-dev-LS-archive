using Core.Interface.IDamageble;
using UnityEngine;

namespace Core.Manager.StageDataManager
{
    public class StageDataManager : Singleton<StageDataManager>
    {
        // 슬롯 리스트

        public void TakeDamage(IDamageble target, int Damage)
        {
            int hp = target.Health -= Damage;

            if (hp <= 0)
            {
                target.Dead();
            }
        }
    }
}


using Monster.Controll;
using Towers.Struct.TowerInfo;
using UnityEngine;

namespace Projectile.Interface
{
    public interface IProjectile
    {
        public void Init(MonsterController target, GameObject keyObj, STowerInfo towerInfo);
        
        public void MoveToTarget(GameObject target);
        public void AtkTarget(GameObject target);
    }
}

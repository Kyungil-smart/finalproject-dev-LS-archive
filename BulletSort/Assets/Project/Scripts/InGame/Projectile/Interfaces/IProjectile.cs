using UnityEngine;

namespace Projectile.Interface
{
    public interface IProjectile
    {
        public void Init(GameObject target, GameObject keyObj, int atk, float moveSpeed);
        
        public void MoveToTarget(GameObject target);
        public void AtkTarget(GameObject target);
    }
}

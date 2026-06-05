using UnityEngine;

namespace Projectile.Interface
{
    public interface IProjectile
    {
        public GameObject Target { get; set; }
        
        public void MoveToTarget(GameObject target);
        public void AtkTarget(GameObject target);
    }
}

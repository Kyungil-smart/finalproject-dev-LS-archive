using Core.Interface.IDamageable;
using UnityEngine;

namespace Monster.Boss
{
   public class BossMonster : MonoBehaviour
   {
      private Timer _normalAtkTimer;

      private void Awake()
      {
         _normalAtkTimer = new Timer(0.8f);
      }

      private void FixedUpdate()
      {
         _normalAtkTimer.UpdateTimer();

         if (_normalAtkTimer.IsEnabled)
            NormalAttack();
      }

      private void NormalAttack()
      {
         BulletAttack();
      }

      private void BulletAttack()
      {
         // 투사체 공격
      }

      private void LockAttack()
      {
      }

      private void DangerAttack()
      {
      }
   }
}

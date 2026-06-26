using System;
using Core.Interface.IDamageable;
using UnityEngine;

namespace Monster.Boss
{
   

   public class BossMonster : MonoBehaviour, IDamageable
   {
      private int _atk;
      private int _maxHealth;
      private int _health;
      
      private bool _isDead;
      
      private Timer _normalAtkTimer;

      private void Awake()
      {
         _normalAtkTimer = new Timer(0.8f);
      }

      private void FixedUpdate()
      {
         _normalAtkTimer.UpdateTimer();
         
         if(_normalAtkTimer.IsEnabled)
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

      public void Init(MonsterData monsterData)
      {
         _atk = monsterData.MonsterAtk;
         _maxHealth =  monsterData.MonsterHp;
         _isDead = false;
         _health = _maxHealth;
      }

      private void Dead()
      {
         _isDead = true;
      }

      public int Health { get; }
      public int MaxHealth { get; }
      public void TakeDamage(int Damage)
      {
         _health -= Damage;

         if (_health <= 0)
         {
            Dead();
         }
      }
   }
}

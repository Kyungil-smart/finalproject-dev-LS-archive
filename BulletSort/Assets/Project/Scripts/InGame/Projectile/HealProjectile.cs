using System;
using System.Collections.Generic;
using Core.Interface.IDamageable;
using Core.ObjectPool;
using Projectile.Interface;
using Core.ObjectPool.Interface;
using InGame.Slot;
using Monster.Controll;
using Towers.Struct.TowerInfo;
using UnityEngine;

namespace Projectile
{
    public class HealProjectile : MonoBehaviour, IProjectile, IPoolable
    {
        private MonsterController _target;
        private GameObject _keyObj;
        private List<GameObject> _atkList;

        private SlotBoardManager _slotBoardManager;
        private List<Slot> _slots;
        
        private float _moveSpeed;
        private int _atk;

        public GameObject KeyObject
        {
            get { return _keyObj; }
            set { _keyObj = value; }
        }
        
        private void Awake()
        {
            _atkList = new List<GameObject>();
            _slots = new List<Slot>();
            _slotBoardManager = FindObjectOfType<SlotBoardManager>();
        }

        private void Start()
        {
            _slots = _slotBoardManager.Slots;
        }

        private void FixedUpdate()
        {
            if (_target.isDead) _target = null;

            if (_target == null)
            {
                PoolManager.Instance.Release(_keyObj, gameObject);
                return;
            }
            
            MoveToTarget(_target?.gameObject);
        }

        public void MoveToTarget(GameObject target)
        {
            if (target == null) return;

            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
                _target.transform.position, _moveSpeed * Time.deltaTime);
        }

        // 투사체가 몬스터에 도달 시
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag == "Monster")
            {
                _atkList.Add(other.gameObject);
                AtkTarget(_atkList[0]);
            }
        }

        public void AtkTarget(GameObject target)
        {
            if (target == null)
            {
                _atkList.Remove(target);
                return;
            }
            // 피해 계산
            target.GetComponent<IDamageable>().TakeDamage(_atk);
            HealSlot();
            PoolManager.Instance.Release(_keyObj, gameObject);
        }

        private void HealSlot()
        {
            int minHP = int.MaxValue;
            Slot lowerHPSlot = null;
            
            foreach (Slot slot in _slots)
            {
                if (slot.Health.Health < minHP)
                {
                    minHP = slot.Health.Health;
                    lowerHPSlot = slot;
                }
            }
            
            // 슬롯HP변경 기능 필요
            lowerHPSlot.Health.HealOnAttack(_atk);
        }

        // 데이터 받아오기
        public void Init(MonsterController target, GameObject keyObj, STowerInfo towerInfo)
        {
            _target = target;
            _keyObj = keyObj;
            _atk = towerInfo.TowerAtk;
            _moveSpeed = towerInfo.BulletSpeed;
        }

        public void OnSpawn()
        {

        }

        public void OnDespawn()
        {
            _target = null;
        }
    }
}

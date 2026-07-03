using System;
using System.Collections.Generic;
using Core.Interface.IDamageable;
using Core.ObjectPool;
using Projectile.Interface;
using Core.ObjectPool.Interface;
using Monster.Controll;
using Towers.Struct.TowerInfo;
using UnityEngine;

namespace Projectile
{
    public class SnipeProjectile : MonoBehaviour, IProjectile, IPoolable
    {
        private MonsterController _target;
        private GameObject _keyObj;
        private List<GameObject> _atkList;
        private HashSet<GameObject> _atkedList;

        private float _moveSpeed;
        private int _atk;
        private int _piercingcount;

        public GameObject KeyObject
        {
            get { return _keyObj; }
            set { _keyObj = value; }
        }

        private void Awake()
        {
            _atkList = new List<GameObject>();
            _atkedList = new HashSet<GameObject>();
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
                if(!_atkedList.Contains(other.gameObject))
                    _atkList.Add(other.gameObject);
                
                AtkTarget(_atkList[0]);
            }
        }

        public void AtkTarget(GameObject target)
        {
            if(target == null) return;
            // 피해 계산
            target.GetComponent<IDamageable>().TakeDamage(_atk);
            --_piercingcount;
            _atkedList.Add(target);
            _atkList.Remove(target);
            if(_piercingcount == 0)
                PoolManager.Instance.Release(_keyObj, gameObject);
        }

        // 데이터 받아오기
        public void Init(MonsterController target, GameObject keyObj, STowerInfo towerInfo)
        {
            _target = target;
            _keyObj = keyObj;
            _atk = towerInfo.TowerAtk;
            _moveSpeed = towerInfo.BulletSpeed;
            _piercingcount = towerInfo.PiercingCount;
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

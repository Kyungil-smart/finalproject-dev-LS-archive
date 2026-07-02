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
    public class ExplosiveProjectile : MonoBehaviour, IProjectile, IPoolable
    {
        private MonsterController _target;
        private GameObject _keyObj;
        private CircleCollider2D _collider;
        private List<GameObject> _atkList;

        private float _moveSpeed;
        private int _atk;
        private float _radius;

        public GameObject KeyObject
        {
            get { return _keyObj; }
            set { _keyObj = value; }
        }

        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            atkList = new List<GameObject>();
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
                Explosion();
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            _atkList.Add(other.gameObject);
        }

        // 폭발 이펙트 재생, 콜라이더 범위 확장
        private void Explosion()
        {
            _collider.radius = _radius;

            // 콜라이더 내 오브젝트 모두 공격
            foreach (GameObject tar in _atkList)
            {
                AtkTarget(tar);
            }
        }

        public void AtkTarget(GameObject target)
        {
            if(target == null) return;
            // 피해 계산
            target.GetComponent<IDamageable>().TakeDamage(_atk);
            PoolManager.Instance.Release(_keyObj, gameObject);
        }

        // 데이터 받아오기
        public void Init(MonsterController target, GameObject keyObj, STowerInfo towerInfo)
        {
            _target = target;
            _keyObj = keyObj;
            _atk = towerInfo.TowerAtk;
            _moveSpeed = towerInfo.BulletSpeed;
            _radius = towerInfo.SplashRadius;
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

using System.Collections.Generic;
using Core;
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
        
        private Vector3 _dir;

        private float _moveSpeed;
        private int _atk;
        private float _radius;
        private bool _isCollision;
        private bool _isExplosive;

        public GameObject KeyObject
        {
            get { return _keyObj; }
            set { _keyObj = value; }
        }

        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _atkList = new List<GameObject>();
        }


        private void FixedUpdate()
        {
            if(_isCollision) return;
            
            if (ScreenWatcher.Instance.IsOutSide(transform.position, 1f))
            {
                PoolManager.Instance.Release(_keyObj, gameObject);
                return;
            }
            
            Move();
        }

        public void MoveToTarget(GameObject target)
        {
        }
        
        private void Move()
        {
            gameObject.transform.position += _dir *(_moveSpeed * Time.deltaTime);
        }

        // 투사체가 몬스터에 도달 시
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(_isCollision) return;
            
            if (other.gameObject.tag == "Monster")
                Explosion();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if(_isExplosive) return;
            
            if(other.gameObject.tag == "Monster")
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
            
            _isCollision = true;
            _isExplosive = true;
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
            _dir = (_target.transform.position - transform.position).normalized;
            _dir.z = 0;
        }

        public void OnSpawn()
        {
            _isCollision = false;
            _isExplosive = false;
        }

        public void OnDespawn()
        {
            _target = null;
        }
    }
}

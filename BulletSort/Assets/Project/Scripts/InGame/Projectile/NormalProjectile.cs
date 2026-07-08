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
    public class NormalProjectile : MonoBehaviour, IProjectile, IPoolable
    {
        enum TowerAIType
        {
            Shotgun = 3 
        }
        
        private MonsterController _target;
        private GameObject _keyObj;
        private List<GameObject> _atkList;
        private Camera _mainCamera;

        private Vector3 _dir;

        private float _moveSpeed;
        private int _atk;
        
        private int _towerAitype;
        
        private bool _isCollision;

        public GameObject KeyObject
        {
            get { return _keyObj; }
            set { _keyObj = value; }
        }
        
        private void Awake()
        {
            _atkList = new List<GameObject>();
            _mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            if(_isCollision) return;
            
            if (_towerAitype == (int)TowerAIType.Shotgun)
            {
                Move();
            }
            
            else
            {
                if (_target.isDead) _target = null;

                if (_target == null)
                {
                    PoolManager.Instance.Release(_keyObj, gameObject);
                    return;
                }

                MoveToTarget(_target?.gameObject);
            }
        }

        public void MoveToTarget(GameObject target)
        {
            if (target == null) return;

            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
                _target.transform.position, _moveSpeed * Time.deltaTime);
        }

        private void Move()
        {
            if (ScreenWatcher.Instance.IsOutSide(transform.position, 1f))
            {
                PoolManager.Instance.Release(_keyObj, gameObject);
                return;
            }
            
            gameObject.transform.position += _dir * (_moveSpeed * Time.deltaTime);
        }
        

        // 투사체가 몬스터에 도달 시
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(_isCollision) return;
            
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
            _isCollision = true;
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
            _towerAitype = towerInfo.TowerAIType;
            _dir = (_target.transform.position - transform.position).normalized;
            _dir.z = 0;
        }

        public void OnSpawn()
        {
            _isCollision = false;
        }

        public void OnDespawn()
        {
            _target = null;
        }
    }
}

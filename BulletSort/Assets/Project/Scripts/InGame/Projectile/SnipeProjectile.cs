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

        private Camera _mainCamera; 
        
        private float _moveSpeed;
        private int _atk;
        private int _piercingcount;
        private Vector3 _dir;

        public GameObject KeyObject
        {
            get { return _keyObj; }
            set { _keyObj = value; }
        }

        private void Awake()
        {
            _mainCamera = Camera.main;
            Clear();
        }

        private void FixedUpdate()
        {
            Vector3 viewPos = _mainCamera.WorldToViewportPoint(_target.transform.position);

            if(viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1)
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
            _dir = (_target.transform.position - transform.position).normalized;
            _dir.z = 0;
        }

        public void OnSpawn()
        {
            Clear();
        }

        public void OnDespawn()
        {
            _target = null;
        }

        private void Clear()
        {
            _atkList = new List<GameObject>();
            _atkedList = new HashSet<GameObject>();
        }
    }
}

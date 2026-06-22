using System.Collections;
using Core.Manager.SpawnManager;
using Core.ObjectPool;
using Core.ObjectPool.Interface;
using InGame.Tower.Data;
using Projectile.Interface;
using Towers.Interface.Tower;
using Towers.Struct.TowerInfo;
using UnityEngine;

namespace Towers.Factory
{
    public class BasicTower : MonoBehaviour, ITower
    {
        private STowerInfo _towerInfo;
        private TargetDetector _targetDetector;
        
        private GameObject _projectile;
        
        private Coroutine _atkCoroutine;
        
        private void Awake()
        {
            _targetDetector = gameObject.GetComponent<TargetDetector>();
        }

        private void Start()
        {
            _towerInfo.ShowData();
            _projectile = SpawnManager.Instance.SpawnProjectile(_towerInfo.ProjectileType, _towerInfo.TowerMaxAmmo);
            
            _atkCoroutine = StartCoroutine(Attack());
        }

        public IEnumerator Attack()
        {
            yield return new WaitUntil(() => _targetDetector.target != null);
            
            for (int i = 0; i < _towerInfo.TowerMaxAmmo; i++)
            {
                GameObject valueObj = PoolManager.Instance.Get(_projectile, gameObject.transform.position, Quaternion.identity);
                valueObj.GetComponent<IProjectile>().Target = _targetDetector.target;
                valueObj.GetComponent<IPoolable>().KeyObject = _projectile;
                yield return new WaitForSeconds(_towerInfo.TowerAtkSpeed);
                yield return new WaitUntil(() => _targetDetector.target != null);
            }
            
            Destroy(gameObject);
        }

        // SO 데이터 저장
        public void SetData(TowerData towerData)
        {
            _towerInfo = new STowerInfo(towerData);
        }

        private void OnDestroy()
        {
            _atkCoroutine = null;
        }
    }
}

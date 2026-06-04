using Core.Manager.SpawnManager;
using Core.ObjectPool;
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
        
        private void Awake()
        {
            _targetDetector = gameObject.GetComponent<TargetDetector>();
            
            // 테스트용 하드코드
            _towerInfo = new STowerInfo("기본형 타워");
        }

        private void Start()
        {
            _towerInfo.ShowData();
            _projectile = SpawnManager.Instance.SpawnProjectile(_towerInfo.ProjectileType, _towerInfo.TowerMaxAmmo);
            //_targetDetector.SetDetectRange();
        }

        private void FixedUpdate()
        {
            Attack();
        }

        // SO데이터 불러오기 구현 필요
        private void Init()
        {
            
        }

        public void Attack()
        {
            if(_targetDetector.target == null) return;

            for (int i = 0; i < _towerInfo.TowerMaxAmmo; i++)
            {
                GameObject obj = PoolManager.Instance.Get(_projectile, gameObject.transform.position, Quaternion.identity);
                obj.GetComponent<IProjectile>().Target = _targetDetector.target;
            }
        }
    }
}

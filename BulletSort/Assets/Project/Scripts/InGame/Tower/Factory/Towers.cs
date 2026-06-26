using System.Collections;
using Core.Manager.SpawnManager;
using Core.ObjectPool;
using InGame.Slot;
using InGame.Tower.Data;
using Projectile.Interface;
using Towers.Interface.Tower;
using Towers.Struct.TowerInfo;
using UnityEngine;

namespace Towers.Factory
{
    public class Towers : MonoBehaviour, ITower
    {
        private STowerInfo _towerInfo;
        private TargetDetector _targetDetector;
        private int _currentAmmo;
        
        private GameObject _projectile;
        
        private SlotTurretQueue _slotTurretQueue;
        
        private Coroutine _atkCoroutine;
        
        public STowerInfo TowerInfo => _towerInfo;
        public int CurrentAmmo => _currentAmmo;
        
        private void Awake()
        {
            _targetDetector = gameObject.GetComponent<TargetDetector>();
        }

        private void Start()
        {
            _slotTurretQueue = GetComponentInParent<SlotTurretQueue>();
            _projectile = SpawnManager.Instance.SpawnProjectile(_towerInfo.ProjectileType, _towerInfo.TowerMaxAmmo);
        }

        public void StartAttack() => StartCoroutine(Attack());

        // 공격 코루틴
        public IEnumerator Attack()
        {
            yield return new WaitUntil(() => _targetDetector.target != null);
            
            for (int i = 0; i < _towerInfo.TowerMaxAmmo; i++)
            {
                GameObject valueObj = PoolManager.Instance.Get(_projectile, gameObject.transform.position, Quaternion.identity);
                
                valueObj.GetComponent<IProjectile>().Init(_targetDetector.target, _projectile, _towerInfo.TowerAtk,
                    10f);
                _currentAmmo--;
                yield return new WaitForSeconds(_towerInfo.TowerAtkSpeed);
                yield return new WaitUntil(() => _targetDetector.target != null);
            }
            
            Destroy(gameObject);
        }

        // Oversorting시 호출해야할 매서드
        // 잔탄발사처리
        public void OnOverSorting()
        {
            // 공격속도 0.1 , 공격력 반감 변경
            _towerInfo.OversortingData();
        }

        // 타워 데이터 셋팅
        // 생성시 SO데이터 저장 or 특전적용시 데이터 갱신
        public void SetData(TowerData towerData)
        {
            _towerInfo = new STowerInfo(towerData);
            _targetDetector.DetectRange = _towerInfo.TowerMaxLange;
            _currentAmmo = towerData.TowerMaxAmmo;
        }

        private void OnDestroy()
        {
            _atkCoroutine = null;
            _currentAmmo = 0;
            _slotTurretQueue.NotifyTurretDestroyed(this);
        }
    }
}

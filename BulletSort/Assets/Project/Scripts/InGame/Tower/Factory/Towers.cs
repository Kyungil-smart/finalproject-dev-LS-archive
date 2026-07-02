using Core.Manager.SpawnManager;
using Core.ObjectPool;
using Ingame.Perks;
using InGame.Slot;
using InGame.Tower.Data;
using Projectile.Interface;
using System.Collections;
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
        private bool _isOverSorting;

        private GameObject _projectile;

        private SlotTurretQueue _slotTurretQueue;

        private Coroutine _atkCoroutine;

        public STowerInfo TowerInfo => _towerInfo;
        public int CurrentAmmo => _currentAmmo;

        private EffectManager _effectManager;

        private EffectBonusValue _effectBonusValue;

        private int MaxAmmo { get { return _towerInfo.TowerMaxAmmo + _effectBonusValue.BonusMaxAmmo; } }
        private int Atk { get { return _towerInfo.TowerAtk + _effectBonusValue.BonusATK; } }
        private float AtkSpeed { get { return _towerInfo.TowerAtkSpeed * (1 - _effectBonusValue.BonusATKSpeed); } }

        private void Awake()
        {
            _isOverSorting = false;
            _targetDetector = gameObject.GetComponent<TargetDetector>();
        }

        private void Start()
        {
            _slotTurretQueue = GetComponentInParent<SlotTurretQueue>();
            _effectManager = FindAnyObjectByType<EffectManager>();
            _effectBonusValue = _effectManager.GetBonusValueByTowerInfo(_towerInfo);
            _projectile = SpawnManager.Instance.SpawnProjectile(_towerInfo.ProjectileType, _towerInfo.TowerMaxAmmo);
        }

        public void StartAttack()
        {
            if (!gameObject.activeInHierarchy) return;

            StartCoroutine(Attack());
        }

        // 공격 코루틴
        public IEnumerator Attack()
        {
            if (!_isOverSorting)
                yield return new WaitUntil(() => _targetDetector.target != null);


            int maxAmmo = MaxAmmo;

            for (int i = 0; i < MaxAmmo; i++)
            {
                if (_targetDetector.target != null)
                {
                    GameObject valueObj = PoolManager.Instance.Get(_projectile, gameObject.transform.position, Quaternion.identity);

                    valueObj.GetComponent<IProjectile>().Init(_targetDetector.target, _projectile, _towerInfo);
                }

                Debug.Log($"<color=red> origin ATK : {_towerInfo.TowerAtk}, Total ATK : {Atk}</color>");

                if (maxAmmo < MaxAmmo)
                {
                    _currentAmmo += (MaxAmmo - maxAmmo);
                    maxAmmo = MaxAmmo;
                }

                _currentAmmo--;
                yield return new WaitForSeconds(AtkSpeed);

                if (!_isOverSorting)
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
            _isOverSorting = true;
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

using Core.Manager.SpawnManager;
using Core.ObjectPool;
using Ingame.Perks;
using InGame.Slot;
using Projectile.Interface;
using System.Collections;
using Towers.Interface.Tower;
using Towers.Struct.TowerInfo;
using UnityEngine;

namespace Towers.Factory
{
    public class Towers : MonoBehaviour, ITower
    {
        private STowerInfo _towerInfoOrigin;
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

        private void Awake()
        {
            _isOverSorting = false;
            _targetDetector = gameObject.GetComponent<TargetDetector>();
            _effectManager = FindAnyObjectByType<EffectManager>();
        }

        private void OnEnable()
        {
            _effectManager.OnEffectApply += ApplyEffect;
        }

        private void Start()
        {
            _slotTurretQueue = GetComponentInParent<SlotTurretQueue>();
            _projectile = SpawnManager.Instance.SpawnProjectile(_towerInfoOrigin.ProjectileType, _towerInfoOrigin.TowerMaxAmmo);
            _targetDetector.towerType = _towerInfoOrigin.TowerType;
        }

        private void OnDisable()
        {
            _effectManager.OnEffectApply -= ApplyEffect;
        }

        public void StartAttack()
        {
            if (!gameObject.activeInHierarchy) return;

            if (_atkCoroutine != null)
            {
                StopCoroutine(_atkCoroutine);
                _atkCoroutine = null;
            }

            _atkCoroutine = StartCoroutine(Attack());
        }

        // 공격 코루틴
        public IEnumerator Attack()
        {
            if (!_isOverSorting)
                yield return new WaitUntil(() => _targetDetector.target != null);


            int maxAmmo = _towerInfo.TowerMaxAmmo;

            for (int i = 0; i < maxAmmo; i++)
            {
                if (_targetDetector.target != null)
                {
                    GameObject valueObj = PoolManager.Instance.Get(_projectile, gameObject.transform.position, Quaternion.identity);

                    valueObj.GetComponent<IProjectile>().Init(_targetDetector.target, _projectile, _towerInfo);
                }

                if (maxAmmo < _towerInfo.TowerMaxAmmo)
                {
                    _currentAmmo += (_towerInfo.TowerMaxAmmo - maxAmmo);
                    maxAmmo = _towerInfo.TowerMaxAmmo;
                }

                _currentAmmo--;
                yield return new WaitForSeconds(_towerInfo.TowerAtkSpeed);

                if (!_isOverSorting)
                    yield return new WaitUntil(() => _targetDetector.target != null);
            }
            SpendAllAmmo();
        }

        private void SpendAllAmmo()
        {
            if (_atkCoroutine != null)
            {
                StopCoroutine(_atkCoroutine);
                _atkCoroutine = null;
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
            _towerInfoOrigin = new STowerInfo(towerData);
            _targetDetector.DetectRange = _towerInfoOrigin.TowerMaxLange;
            _currentAmmo = towerData.TowerMaxAmmo;

            _towerInfo = _towerInfoOrigin;

            ApplyEffect(_effectManager.GroupEffectBonus[(TowerGroupType)_towerInfoOrigin.TowerType]);

        }

        private void ApplyEffect(EffectBonusValue bonus)
        {
            if (bonus.type != (TowerGroupType)_towerInfoOrigin.TowerType)
            {
                return;
            }

            _towerInfo = _towerInfoOrigin.UpdateInfo(bonus);
        }


        private void OnDestroy()
        {
            _currentAmmo = 0;
            _slotTurretQueue.NotifyTurretDestroyed(this);
        }
    }
}

using Core;
using Ingame.Perks;
using System;

namespace Towers.Struct.TowerInfo
{
    [Serializable]
    // SO에서 데이터를 받아오기 용
    public struct STowerInfo
    {
        private int _towerID;

        private int _towerType;

        // 타겟팅 조건
        private int _towerAIType;

        private int _towerAtk;

        private float _towerAtkSpeed;

        // 사거리
        private float _towerMaxLange;

        private int _towerMaxAmmo;

        private int _projectileCount;

        private float _projectileSize;

        private int _piercingCount;

        private float _splashRadius;

        private int _currentHp;

        private float _bulletSpeed;

        // 투사체 종류
        private EProjectileType _towerProjectile;

        public EProjectileType ProjectileType => _towerProjectile;

        public int TowerType => _towerType;
        public int TowerAIType => _towerAIType;
        public int TowerAtk => _towerAtk;
        public float TowerAtkSpeed => _towerAtkSpeed;
        public float TowerMaxLange => _towerMaxLange;
        public int TowerMaxAmmo => _towerMaxAmmo;

        public int PiercingCount => _piercingCount;
        public float SplashRadius => _splashRadius;
        public int CurrentHp => _currentHp;

        public float BulletSpeed => _bulletSpeed;

        public STowerInfo(TowerData towerData)
        {
            _towerID = towerData.TowerID;
            _towerType = towerData.TowerType;
            _towerAIType = towerData.TowerAiType;
            _towerProjectile = (EProjectileType)towerData.TowerProjectile;
            _towerAtk = towerData.TowerAtk;
            _towerAtkSpeed = towerData.TowerAtkSpeed;
            _towerMaxLange = towerData.TowerMaxRange;
            _towerMaxAmmo = towerData.TowerMaxAmmo;
            _projectileCount = towerData.ProjectileCount;
            _projectileSize = towerData.ProjectileSize;
            _piercingCount = towerData.PiercingCount;
            _splashRadius = towerData.SplashRadius;
            _currentHp = towerData.CurrentHp;
            _bulletSpeed = towerData.BulletSpeed;
        }

        public void OversortingData()
        {
            _towerAtkSpeed = 0.1f;
            _towerAtk /= 2;
        }

        public STowerInfo UpdateInfo(EffectBonusValue bonus)
        {
            STowerInfo changedInfo = new STowerInfo(DataManager.Instance.GetData<TowerData>(this._towerID));

            // 폭발형 처리
            if (_towerType == 4)
            {
                changedInfo._towerAtk += _towerAtk * (bonus.BonusATK / 100);
            }
            else
            {
                changedInfo._towerAtk += bonus.BonusATK;
            }

            changedInfo._towerAtkSpeed = (_towerAtkSpeed * (1 - bonus.BonusATK));

            changedInfo._projectileCount = _projectileCount + bonus.BonusShotProjCount;
            changedInfo._towerMaxAmmo = _towerMaxAmmo + bonus.BonusMaxAmmo;

            changedInfo._projectileSize = _projectileSize + _projectileSize * bonus.ProjSize;
            changedInfo._piercingCount += bonus.BonusProjPiercing;

            if (_towerType == 1 || _towerType == 4)
            {
                changedInfo._splashRadius = _splashRadius + _splashRadius * bonus.BonusBuffValue;
            }

            //bonus.BonusBuffValue  // 기타 값 (범위 증가, 힐량 증가 등)

            return changedInfo;
        }
    }
}


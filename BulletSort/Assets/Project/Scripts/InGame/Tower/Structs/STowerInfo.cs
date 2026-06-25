using System;
using InGame.Tower.Data;
using UnityEngine;

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
        private int _towerMaxLange;

        private int _towerMaxAmmo;

        private int _projectileCount;

        private float _projectileSize;

        private int _piercingCount;

        private float _splashRadius;

        private int _currentHp;
        
        // 투사체 종류
        private EProjectileType _towerProjectile;
        
        public EProjectileType ProjectileType => _towerProjectile;
        
        public int TowerType => _towerType;
        public int TowerAtk => _towerAtk;
        public float TowerAtkSpeed => _towerAtkSpeed;
        public int TowerMaxLange => _towerMaxLange;
        public int TowerMaxAmmo => _towerMaxAmmo;

        public STowerInfo(TowerData towerData)
        {
            _towerID = towerData.TowerID;
            _towerType = towerData.TowerType;
            _towerAIType = towerData.TowerAIType;
            _towerProjectile = (EProjectileType)towerData.TowerProjectile;
            _towerAtk = towerData.TowerAtk;
            _towerAtkSpeed = towerData.TowerAtkSpeed;
            _towerMaxLange =  towerData.TowerMaxLange;
            _towerMaxAmmo = towerData.TowerMaxAmmo;
            _projectileCount = towerData.ProjectileCount;
            _projectileSize = towerData.ProjectileSize;
            _piercingCount = towerData.PiercingCount;
            _splashRadius = towerData.SplashRadius;
            _currentHp = towerData.CurrentHp;
        }

        public void OversortingData()
        {
            _towerAtkSpeed = 0.1f;
            _towerAtk /= 2;
        }
    }
}


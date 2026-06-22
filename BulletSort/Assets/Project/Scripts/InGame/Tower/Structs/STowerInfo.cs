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
        
        private int _towerAtk;
        
        private float _towerAtkSpeed;

        private int _towerMaxAmmo;

        private int _projectileCount;
        
        // 투사체 종류
        private EProjectileType _towerAIType;
        
        // 포탑 유형 클래스 스트럭트
            //  타겟팅

        // 사격 타입
        
        public EProjectileType ProjectileType => _towerAIType;
        
        public int TowerAtk => _towerAtk;
        public float TowerAtkSpeed => _towerAtkSpeed;
        public int TowerMaxAmmo => _towerMaxAmmo;
        

        // 테스트용 임시 데이터
        public STowerInfo(TowerData towerData)
        {
            _towerID = towerData.TowerID;
            _towerAtk = towerData.TowerAtk;
            _towerAtkSpeed = towerData.TowerAtkSpeed;
            _projectileCount = towerData.ProjectileCount;
            _towerAIType = (EProjectileType)towerData.TowerAIType;
            _towerMaxAmmo = towerData.TowerMaxAmmo;
        }

        // 디버그용 코드
        public void ShowData()
        {
            Debug.Log($"타워 공격력 : {_towerAtk}");
            Debug.Log($"타워 공격속도 : {_towerAtkSpeed}");
            Debug.Log($"타워 투사체발사개수 : {_projectileCount}");
            Debug.Log($"타워 투사체종류 : {_towerAIType}");
            Debug.Log($"타워 최대발사횟수 : {_towerMaxAmmo}");
        }
        
        // 상태이상 종류 클래스 스트럭트
        
        // 데이터 수정용 메서드
    }
}


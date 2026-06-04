using System;
using UnityEngine;

namespace Towers.Struct.TowerInfo
{
    [Serializable]
    // SO에서 데이터를 받아오기 용
    public struct STowerInfo
    {
        private string _towerName;
        
        private int _towerAtk;
        private float _towerAtkSpeed;

        private int _towerMaxAmmo;

        private int _projectileCount;
        
        // 투사체 종류
        private EProjectileType _projectileType;
        
        // 포탑 유형 클래스 스트럭트
            //  타겟팅

        // 사격 타입
        
        public EProjectileType ProjectileType => _projectileType;
        
        public int TowerMaxAmmo => _towerMaxAmmo;
        

        // 테스트용 임시 데이터
        public STowerInfo(string towerName)
        {
            _towerName = towerName;
            _towerAtk = 100;
            _towerAtkSpeed = 4;
            _projectileCount = 1;
            _projectileType = EProjectileType.Normal;
            _towerMaxAmmo = 20;
        }

        // 디버그용 코드
        public void ShowData()
        {
            Debug.Log($"타워 이름 : {_towerName}");
            Debug.Log($"타워 공격력 : {_towerAtk}");
            Debug.Log($"타워 공격속도 : {_towerAtkSpeed}");
            Debug.Log($"타워 투사체발사개수 : {_projectileCount}");
            Debug.Log($"타워 투사체종류 : {_projectileType}");
            Debug.Log($"타워 최대발사횟수 : {_towerMaxAmmo}");
        }
        
        // 상태이상 종류 클래스 스트럭트
        
        // 데이터 수정용 메서드
    }
}


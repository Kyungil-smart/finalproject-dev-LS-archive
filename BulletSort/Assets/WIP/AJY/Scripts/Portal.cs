using System;
using Core.Manager.SpawnManager;
using UnityEngine;
using Monster.Spawn;

namespace Monster.Portal
{
    public class Portal : MonoBehaviour
    {
        public SpawnPoint[] spawnPoints;
        
        // 웨이브 타이머와 연동작업이 필요한 부분
        // 임시로 Portal이 타이머를 가지게 처리
        private Timer _spawnTimer;
        
        // 웨이브 종료시 초기화 필요
        private int _spawnPase;
        
        private int _spawnCount;
        private int _maxMonsterCount;
        
        public int MaxMonsterCount => _maxMonsterCount;

        private void Awake()
        {
            spawnPoints = gameObject.GetComponentsInChildren<SpawnPoint>();
            _spawnTimer = new Timer(5);
            
            // 웨이브 정보 불러오기
            //StageManager.Instance
            _spawnPase = 0;
            //임시
            _spawnCount = 5;
            // 총 소환할 몬스터 / 8 
            // 40
        }

        private void Update()
        {
            if (_spawnPase >= 7) return;
            
            _spawnTimer.UpdateTimer();
            
            if(_spawnTimer.IsEnabled)
            {
                // 일정 주기마다 스폰
                SpawnManager.Instance.SpawnMonster(_spawnCount);    
                _spawnPase++;
                Debug.Log("스폰 시작");
                
                _spawnTimer.ResetTimer(5);
            }
        }

        // 스폰페이즈 초기화
        public void ResetPhase()
        {
            _spawnPase = 0;
        }
    }
}

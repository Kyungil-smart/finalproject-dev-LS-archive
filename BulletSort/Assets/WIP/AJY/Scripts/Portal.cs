using System.Collections;
using Core;
using Core.Manager.SpawnManager;
using Monster.Spawn;
using UnityEngine;

namespace Monster.Portal
{
    public class Portal : MonoBehaviour
    {
        public SpawnPoint[] spawnPoints;
        public Transform bossZone;

        // 웨이브 종료시 초기화 필요
        private int _spawnPase;

        private int _spawnCount;

        public void StartSpawn() => StartCoroutine(Spawncouroutine());

        private void Awake()
        {
            spawnPoints = gameObject.GetComponentsInChildren<SpawnPoint>();
            _spawnPase = 0;
        }

        private IEnumerator Spawncouroutine()
        {
            if(StageManager.Instance.IsBossWave)
                SpawnManager.Instance.SpawnBoss();

            else
            {
                while (_spawnPase < 7)
                {
                     Spawn();
                    yield return new WaitForSeconds(5);
                }
            }
        }

        private void Spawn()
        {
            int normalgroupID = StageManager.Instance.NormalMonsterGroup;
            int normalspawnCount = StageManager.Instance.NormalSpawnCount;
            int speedygroupID = StageManager.Instance.SpeedyMonsterGroup;
            int speedyspawnCount = StageManager.Instance.SpeedySpawnCount;
            int tankergroupID = StageManager.Instance.TankerMonsterGroup;
            int tankerspawnCount = StageManager.Instance.TankerSpawnCount;
            
            // 일정 주기마다 스폰
            SpawnManager.Instance.SpawnMonster(normalgroupID, normalspawnCount);
            SpawnManager.Instance.SpawnMonster(speedygroupID, speedyspawnCount);
            SpawnManager.Instance.SpawnMonster(tankergroupID, tankerspawnCount);
            _spawnPase++;
        }

        // 스폰페이즈 초기화
        public void ResetPhase()
        {
            _spawnPase = 0;
            StopCoroutine(Spawncouroutine());
        }
    }
}

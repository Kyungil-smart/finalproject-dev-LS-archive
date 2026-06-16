using InGame.Slot;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// 현재 Ingame에서 진행중인 Stage의 진행을 관리하는 클래스이다.
    /// Lobby에서 Stage 선택 시 해당 Stage의 index 정보를 받고,
    /// StageData SO의 id로 정보를 접근함.
    /// 
    /// 작성자 : 김경민
    /// </summary>

    // DataManager로부터 현 Stage에 맞는 SO instance를 가져오고
    // Monster Spawner에 요청하는 형태?

    class StageManager : Singleton<StageManager>
    {
        [SerializeField] private SlotBoardManager _slotBoardManager;
        TestMonsterSpawner[] spawners;

        int _targetKillNum = 0;
        int _killCount = 0;

        bool _isWin = false;
        bool _isDefeat = false;

        int _curStageID;
        StageData _stageData;

        int _waveIdx = 0;

        protected override void Init()
        {
            spawners = FindObjectsByType<TestMonsterSpawner>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None);

            foreach (TestMonsterSpawner spawner in spawners)
            {
                _targetKillNum += spawner.MaxMonsterCount;
            }

            // _stageData = DataManager.Instance.GetData<StageData>(_curStageID);
        }

        private void ResetState()
        {
            _waveIdx = 0;
            _isWin = false;
            _isDefeat = false;

            _killCount = 0;
            _targetKillNum = 0;

            foreach (TestMonsterSpawner spawner in spawners)
            {
                _targetKillNum += spawner.MaxMonsterCount;
            }
        }

        private void LoadWaveData()
        {
            //WaveData;
            //WavePatterData;
            //MonsterSpawn;
        }

        private void WaveClearHandler()
        {
            // Process Perks System;
        }

        private void WaveFailHandler()
        {
            // 
        }

        private void FixedUpdate()
        {
            while (_isWin == false && _isDefeat == false)
            {
                if (_targetKillNum == _killCount)   // 제한 시간이 종료되었을 때도 클리어.
                {
                    _isWin = true;
                }

                _isDefeat = CheckDefeatCondition();

                if (_isWin)
                {
                    // 승리 처리
                    // 다음 웨이브 진행
                    break;
                }

                if (_isDefeat)
                {
                    // 패배 처리
                    break;
                }
            }
        }

        private bool CheckDefeatCondition()
        {
            foreach (Slot slot in _slotBoardManager.Slots)
            {
                if (slot.Health.isDead == false)
                {
                    return false;
                }
            }

            return true;
        }

        public void IncrementKillCount()
        {
            _killCount++;
        }
    }
}
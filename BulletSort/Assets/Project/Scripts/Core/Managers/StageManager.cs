using Monster.Portal;

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
    // MonsterGroup을 Spawner에 요청하는 형태

    class StageManager : Singleton<StageManager>
    {
        //[SerializeField] SlotBoardManager _slotBoardManager;

        Portal[] spawners;

        int _targetKillNum = 0;
        int _killCount = 0;

        bool _isWin = false;
        bool _isDefeat = false;

        int _curStageID;
        StageData _stageData;
        WaveData _waveData;

        int _waveIdx;

        private Timer _waveTimer; // 40초;

        protected override void Init()
        {
            //spawners = FindObjectsByType<Portal>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None);

            _waveTimer = new Timer(40);

            // 임시 ID
            _curStageID = 1001;

            _stageData = DataManager.Instance.GetData<StageData>(_curStageID);
            _waveData = DataManager.Instance.GetData<WaveData>(_stageData.WaveDataID);

            _waveIdx = 0;

            PerksManager.Instance.OnPerkPhaseEnded += ResetState;
        }

        // Lobby에서 선택 시 호출할 것
        public void SetStageID(int stageID)
        {
            _curStageID = stageID;
        }

        private void ResetState()
        {
            if (_waveIdx == 9)
            {
                // Boss Wave;
            }

            if (_waveIdx > 9)
            {
                // Stage Clear
            }

            _isWin = false;
            _isDefeat = false;

            _killCount = 0;
            _targetKillNum = 0;

            //foreach (Portal spawner in spawners)
            //{
            //    _targetKillNum += spawner.MaxMonsterCount;
            //}
        }

        private void SpawnCurWavePattern()
        {
            WavePatternData curPattern = DataManager.Instance.GetData<WavePatternData>(_waveData[_waveIdx]);

            //SpawnManager.Instance.SpawnMonster(_stageData.MonsterGroupID_Normal, curPattern.Normal_Count);
            //SpawnManager.Instance.SpawnMonster(_stageData.MonsterGroupID_Speedy, curPattern.Speedy_Count);
            //SpawnManager.Instance.SpawnMonster(_stageData.MonsterGroupID_Tanker, curPattern.Tanker_Count);
        }

        private void WaveClearHandler()
        {
            _waveIdx++;
            PerksManager.Instance.EnterPerksPhase();
        }

        private void WaveFailHandler()
        {
            // 패배 UI 띄우기
        }

        private void FixedUpdate()
        {
            if (_isWin)
            {
                WaveClearHandler();
                return;
            }

            if (_isDefeat)
            {
                WaveFailHandler();
                return;
            }

            _waveTimer.UpdateTimer();

            if (_waveTimer.IsEnabled)
            {
                _isWin = true;
                _waveTimer.ResetTimer(40);
            }

            _isDefeat = CheckDefeatCondition();
        }

        private bool CheckDefeatCondition()
        {
            //foreach (Slot slot in _slotBoardManager.Slots)
            //{
            //    if (slot.Health.isDead == false)
            //    {
            //        return false;
            //    }
            //}

            //return true;

            return false;
        }

        public void IncrementKillCount()
        {
            _killCount++;
        }
    }
}
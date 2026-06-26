using Core.Manager.SpawnManager;
using InGame.Slot;
using Monster.Portal;
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
    // MonsterGroup을 Spawner에 요청하는 형태

    class StageManager : Singleton<StageManager>
    {
        Portal[] spawners;

        int _targetKillNum = 0;
        int _killCount = 0;

        bool _isRunning = false;

        bool _isWin = false;
        bool _isDefeat = false;

        int _curStageID = 1001;
        StageData _stageData;
        WaveData _waveData;

        int _waveIdx;
        public bool IsBossWave { get { return _waveIdx == 9; } }

        private Timer _waveTimer; // 40초;

        public int NormalMonsterGroup { get; private set; }
        public int SpeedyMonsterGroup { get; private set; }
        public int TankerMonsterGroup { get; private set; }

        public int NormalSpawnCount { get; private set; }
        public int SpeedySpawnCount { get; private set; }
        public int TankerSpawnCount { get; private set; }

        public int BossID { get; private set; }

        private SlotBoardManager _slotBoardManager;

        protected override void Init()
        {
            //spawners = FindObjectsByType<Portal>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None);

            _waveTimer = new Timer(40);

            PerksManager.Instance.OnPerkPhaseEnded += ResetState;
        }

        // Lobby에서 선택 시 호출할 것
        public void SetStageID(int stageID)
        {
            _curStageID = stageID;

            _waveIdx = 0;
            _killCount = 0;
            _targetKillNum = 0;

            _stageData = DataManager.Instance.GetData<StageData>(_curStageID);
            _waveData = DataManager.Instance.GetData<WaveData>(_stageData.WaveDataID);

            BossID = _stageData.BossID;

            ResetState();
        }

        public void EnterStage()
        {
            _isRunning = true;
        }

        private void ResetState()
        {
            _isWin = false;
            _isDefeat = false;

            if (_waveIdx > 9)
            {
                // Stage Clear
            }
            else
            {
                SetValueByCurWavePattern();
            }

            SpawnManager.Instance.WaveSpawn();
        }

        public void BindSlotBoardManager(SlotBoardManager slotBoardManager)
        {
            _slotBoardManager = slotBoardManager;
            _slotBoardManager.OnAllSlotsDestroyed += DefeatEvent;
        }

        private void SetValueByCurWavePattern()
        {
            WavePatternData curPattern = DataManager.Instance.GetData<WavePatternData>(_waveData[_waveIdx]);

            NormalMonsterGroup = _stageData.MonsterGroupID_Normal;
            SpeedyMonsterGroup = _stageData.MonsterGroupID_Speedy;
            TankerMonsterGroup = _stageData.MonsterGroupID_Tanker;

            NormalSpawnCount = curPattern.Normal_Count;
            SpeedySpawnCount = curPattern.Speedy_Count;
            TankerSpawnCount = curPattern.Tanker_Count;

            _targetKillNum += (NormalSpawnCount + SpeedySpawnCount + TankerSpawnCount);
            Debug.Log($"Target Kill Num in This Wave : {_targetKillNum}");
        }

        private void WaveClearHandler()
        {
            _waveIdx++;
            PerksManager.Instance.EnterPerksPhase();
        }

        private void WaveFailHandler()
        {
            Time.timeScale = 0;
            // 패배 UI 띄우기
        }

        private void FixedUpdate()
        {
            if (_isRunning)
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

                if (_waveTimer.IsEnabled || _killCount >= _targetKillNum)
                {
                    _isWin = true;
                    _waveTimer.ResetTimer(40);
                }
            }
        }

        private void DefeatEvent()
        {
            _isDefeat = true;
        }

        public void IncrementKillCount()
        {
            _killCount++;
            Debug.Log($"Kill Count : {_killCount}/{_targetKillNum}");
        }
    }
}
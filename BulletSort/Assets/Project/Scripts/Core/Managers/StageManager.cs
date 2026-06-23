using Monster.Portal;

namespace Core
{
    /// <summary>
    /// ���� Ingame���� �������� Stage�� ������ �����ϴ� Ŭ�����̴�.
    /// Lobby���� Stage ���� �� �ش� Stage�� index ������ �ް�,
    /// StageData SO�� id�� ������ ������.
    /// 
    /// �ۼ��� : ����
    /// </summary>

    // DataManager�κ��� �� Stage�� �´� SO instance�� ��������
    // Monster Spawner�� ��û�ϴ� ����?

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

        int _waveIdx = 0;

        private Timer _waveTimer; // 40초;

        protected override void Init()
        {
            //spawners = FindObjectsByType<Portal>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None);

            _waveTimer = new Timer(40);

            // 임시 ID
            _curStageID = 1001;

            _stageData = DataManager.Instance.GetData<StageData>(_curStageID);

            PerksManager.Instance.OnPerkPhaseEnded += ResetState;
        }

        private void ResetState()
        {
            _waveIdx++;

            // _waveIdx가 max가 되면 Stage Clear.
            /*
                Todo.
             */
            //

            _isWin = false;
            _isDefeat = false;

            _killCount = 0;
            _targetKillNum = 0;

            //foreach (Portal spawner in spawners)
            //{
            //    _targetKillNum += spawner.MaxMonsterCount;
            //}
        }

        private void LoadWaveData()
        {
            //WaveData waveData = ;
            //WavePatternData;
            //MonsterSpawn;
        }

        private void WaveClearHandler()
        {
            PerksManager.Instance.EnterPerksPhase();
        }

        private void WaveFailHandler()
        {
            // 
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
                // 패배 처리
                return;
            }

            _waveTimer.UpdateTimer();

            if (_waveTimer.IsEnabled)
            {
                _isWin = true;
                _waveTimer.ResetTimer(40);
            }

            //if (_targetKillNum == _killCount)   // ���� �ð��� ����Ǿ��� ���� Ŭ����.
            //{
            //    _isWin = true;
            //}

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
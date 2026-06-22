using InGame.Slot;
using Monster.Portal;
using UnityEngine;

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
        [SerializeField] private SlotBoardManager _slotBoardManager;
        Portal[] spawners;

        int _targetKillNum = 0;
        int _killCount = 0;

        bool _isWin = false;
        bool _isDefeat = false;

        int _curStageID;
        StageData _stageData;

        int _waveIdx = 0;

        protected override void Init()
        {
            spawners = FindObjectsByType<Portal>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None);

            foreach (Portal spawner in spawners)
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

            foreach (Portal spawner in spawners)
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
                if (_targetKillNum == _killCount)   // ���� �ð��� ����Ǿ��� ���� Ŭ����.
                {
                    _isWin = true;
                }

                _isDefeat = CheckDefeatCondition();

                if (_isWin)
                {
                    // �¸� ó��
                    // ���� ���̺� ����
                    break;
                }

                if (_isDefeat)
                {
                    // �й� ó��
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
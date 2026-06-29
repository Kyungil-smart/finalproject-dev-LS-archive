using Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ingame.ExpSystem
{
    enum EXP_DATA_ID
    {
        NORMAL = 81,
        ELITE = 82
    }

    class ExpManager : MonoBehaviour
    {
        public event Action OnLevelUp;
        public event Action<int> OnExpChanged;

        int _curExp;
        int _curLevel;

        IReadOnlyDictionary<int, LevelData> _levelDataTable;

        int _normalMonsterExp;
        int _eliteMonsterExp;

        private int _curLevelID { get { return _curLevel + 10; } }
        private int _nextLevelID { get { return _curLevel + 11; } }

        private void Awake()
        {
            _curExp = 0;
            _curLevel = 1;

            _normalMonsterExp = DataManager.Instance.GetData<ExpData>((int)EXP_DATA_ID.NORMAL).ExpAmount;
            _eliteMonsterExp = DataManager.Instance.GetData<ExpData>((int)EXP_DATA_ID.ELITE).ExpAmount;

            _levelDataTable = DataManager.Instance.GetTable<LevelData>();
        }

        public void IncrementExp(int monsterType)
        {
            // MonsterType에 따라 증가하는 EXP 양 변화
            // 일반형 : normal, Speedy or Tanker : Elite

            // 임시 코드

            if (_levelDataTable[_curLevelID].IsMaxLevel == 0)
            {
                _curExp += _normalMonsterExp;

                while (_curExp >= _levelDataTable[_nextLevelID].RequiredXP)
                {
                    LevelUp();
                }

                OnExpChanged(_curExp); // Ingame 상 경험치 UI 변화
            }
        }

        private void LevelUp()
        {
            _curLevel++;
            _curExp -= _levelDataTable[_nextLevelID].RequiredXP;

            OnLevelUp();    // 특전 선택 횟수 증가, Ingame 상 레벨 UI 변화

            return;
        }
    }
}

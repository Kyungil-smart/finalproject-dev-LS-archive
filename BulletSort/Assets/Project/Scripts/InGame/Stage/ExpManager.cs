using Core;
using Monster.Controll;
using System;
using System.Collections;
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
        public static event Action OnLevelUp;
        public static event Action<int> OnExpChanged;

        int _curExp;
        int _curLevel;

        IReadOnlyDictionary<int, LevelData> _levelDataTable;

        int _normalMonsterExp;
        int _eliteMonsterExp;

        private int _curLevelID { get { return _curLevel + 10; } }
        private int _nextLevelID { get { return _curLevel + 11; } }

        private WaitForSeconds timer_5s;

        private void Awake()
        {
            _curExp = 0;
            _curLevel = 1;

            _normalMonsterExp = DataManager.Instance.GetData<ExpData>((int)EXP_DATA_ID.NORMAL).ExpAmount;
            _eliteMonsterExp = DataManager.Instance.GetData<ExpData>((int)EXP_DATA_ID.ELITE).ExpAmount;

            _levelDataTable = DataManager.Instance.GetTable<LevelData>();

            timer_5s = new WaitForSeconds(5.0f);
        }
        private void OnEnable()
        {
            MonsterController.OnDead += MonsterDeadHandler;
        }
        private void OnDisable()
        {
            MonsterController.OnDead += MonsterDeadHandler;
        }

        private void MonsterDeadHandler(int monsterType)
        {
            //StartCoroutine(DelayAndIncrementExp(monsterType));
            IncrementExp(monsterType);
        }

        private IEnumerator DelayAndIncrementExp(int monsterType)
        {
            yield return timer_5s;

            IncrementExp(monsterType);
        }

        private void IncrementExp(int monsterType)
        {
            // MonsterType에 따라 증가하는 EXP 양 변화
            // 일반형 : normal, Speedy or Tanker : Elite

            // 임시 코드

            if (_levelDataTable[_curLevelID].IsMaxLevel == 0)
            {
                int prevExp = _curExp;
                if (monsterType == 0)
                {
                    _curExp += _normalMonsterExp;
                }
                else if (monsterType == 1 || monsterType == 2)
                {
                    _curExp += _eliteMonsterExp;
                }

                while (_levelDataTable[_curLevelID].IsMaxLevel == 0 && _curExp >= _levelDataTable[_nextLevelID].RequiredXP)
                {
                    LevelUp();
                }

                //Debug.Log($"경험치 변동 : {prevExp}->{_curExp}");
                OnExpChanged(_curExp); // Ingame 상 경험치 UI 변화
            }
        }

        private void LevelUp()
        {
            _curExp -= _levelDataTable[_nextLevelID].RequiredXP;
            _curLevel++;

            //Debug.Log($"레벨 업 : LV.{_curLevel}");
            OnLevelUp();    // 특전 선택 횟수 증가, Ingame 상 레벨 UI 변화

            return;
        }
    }
}

using InGame.Slot.Data;
using InGame.Sort.Data;
using InGame.Tower.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Core
{
    // 요약 : Unity Process 시작 시 SO asset들을 메모리에 불러와 instance화 하여
    //        Contents Script에서 사용할 수 있도록 관리하는 Manager 코드
    //        각 Type의 Dictionary를 묶어 root Dictionary로 관리하고 개별 Type은 ID로 접근한다.
    // 작성자 : 김경민

    public class DataManager : Singleton<DataManager>
    {
        private Dictionary<Type, object> _rootDict = new Dictionary<Type, object>();

        private List<string> filePaths = new List<string>();

        // 코드 자동 생성중이므로 Init 함수 직접 수정 금지
        protected override void Init()
        {

            // Auto Written Code By GenerateDataMangerPaths in CSVParser.cs
            // [AUTO GENERATED START]
            LoadTable<EffectData>("SO/EffectData");
            LoadTable<MonsterData>("SO/MonsterData");
            LoadTable<MonsterGroupData>("SO/MonsterGroupData");
            LoadTable<PerkData>("SO/PerkData");
            LoadTable<PieceData>("SO/PieceData");
            LoadTable<RarityData>("SO/RarityData");
            LoadTable<SlotData>("SO/SlotData");
            LoadTable<StageData>("SO/StageData");
            LoadTable<TowerData>("SO/TowerData");
            LoadTable<WaveData>("SO/WaveData");
            LoadTable<WavePatternData>("SO/WavePatternData");
            // [AUTO GENERATED END]
            // ==========================================
            // Auto Written End
        }

        private void LoadTable<T>(string assetPath) where T : ScriptableObject
        {
            Type type = typeof(T);

            T[] assets = Resources.LoadAll<T>(assetPath);

            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning($"DataManager : There is no asset in {assetPath}");
                return;
            }

            Dictionary<int, T> subTable = new Dictionary<int, T>();

            foreach (T asset in assets)
            {
                int id = GetIdFromSO(asset);

                if (!subTable.ContainsKey(id))
                {
                    subTable.Add(id, asset);
                }
                else
                {
                    Debug.LogError($"DataManager : Duplicated ID is Detected, ID : {id} in {type.Name}");
                }
            }

            _rootDict.Add(type, subTable);
            Debug.Log($"DataManager : {type.Name} Data are Loaded Successfully, num : {subTable.Count}");
        }


        private int GetIdFromSO(object soInstance)
        {
            FieldInfo[] fields = soInstance.GetType().GetFields();

            foreach (FieldInfo field in fields)
            {
                if (field.Name.EndsWith("ID"))
                {
                    return (int)field.GetValue(soInstance);
                }
            }

            return 0;
        }


        // Contents Code에서 Data를 얻기 위해 사용하는 함수
        public T GetData<T>(int id) where T : ScriptableObject
        {
            Type type = typeof(T);

            if (_rootDict.TryGetValue(type, out object obj))
            {
                Dictionary<int, T> subTable = obj as Dictionary<int, T>;

                if (subTable != null & subTable.TryGetValue(id, out T data))
                {
                    return data;
                }
            }

            Debug.LogError($"DataManager : {id} is Not Found in {type.Name} Dict");
            return null;
        }

        // 특정 타입의 전체 테이블 반환 — 도메인 Query가 목록·필터·키 순회에 사용.
        // GetData가 단건이라면, 이건 테이블 통째로.
        public IReadOnlyDictionary<int, T> GetTable<T>() where T : ScriptableObject
        {
            Type type = typeof(T);
            if (_rootDict.TryGetValue(type, out object obj))
            {
                Dictionary<int, T> subTable = obj as Dictionary<int, T>;
                if (subTable != null)
                    return subTable;
            }
            Debug.LogError($"DataManager : {type.Name} Table is Not Found");
            return null;
        }
    }
}

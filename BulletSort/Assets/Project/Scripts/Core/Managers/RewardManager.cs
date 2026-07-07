using Core;
using System;
using System.IO;
using UnityEngine;

namespace Reward
{
    [Serializable]
    public class RewardSaveData
    {
        public int Point { get; private set; }

        public void SetPoint(int point)
        {
            Point = point;
        }

        public void AddPoint(int delta)
        {
            Point += delta;
        }

        public void SubPoint(int delta)
        {
            Point -= delta;
        }
    }

    public class RewardManager : Singleton<RewardManager>
    {
        public static event Action<RewardSaveData> OnRewardDataChanged;

        private RewardSaveData _currentData;
        private string _savePath;

        public RewardSaveData CurrentData => _currentData;

        protected override void Init()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "RewardSaveData.json");

            // 게임 시작했으니 세이브 파일 로드하기
            LoadData();
        }

        // ★ 보상 획득 (재화 증가)
        public void AddReward(int point)
        {
            _currentData.AddPoint(point);
            NotifyAndSave();
        }

        // ★ 재화 소비 (구매 등) - 소비 가능한지 체크까지 한 세트
        public bool ConsumeCurrency(int cost)
        {
            if (_currentData.Point < cost)
            {
                return false; // 구매 실패
            }

            _currentData.SubPoint(cost);

            NotifyAndSave();
            return true;
        }

        // 데이터 변경 전파 및 파일 IO 일괄 처리
        private void NotifyAndSave()
        {
            OnRewardDataChanged?.Invoke(_currentData);

            SaveData();
        }

        private void SaveData()
        {
            try
            {
                string json = JsonUtility.ToJson(_currentData, true);
                File.WriteAllText(_savePath, json);
                Debug.Log($"[RewardManager] Reward Data is Saved Successfully, Path : {_savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RewardManager] Reward Data is Failed to Save: {e.Message}");
            }
        }

        private void LoadData()
        {
            if (File.Exists(_savePath))
            {
                try
                {
                    string json = File.ReadAllText(_savePath);
                    _currentData = JsonUtility.FromJson<RewardSaveData>(json);
                    Debug.Log("[RewardManager] Successfully Loaded");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[RewardManager] Load Error : {e.Message}");
                    _currentData = new RewardSaveData();
                }
            }
            else
            {
                Debug.Log("[RewardManager] New Reward File");
                _currentData = new RewardSaveData();
            }

            OnRewardDataChanged?.Invoke(_currentData);
        }
    }

}
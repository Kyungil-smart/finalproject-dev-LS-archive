using Core;
using System;
using System.IO;
using UnityEngine;
using Utils;

namespace Reward
{
    public class RewardManager : Singleton<RewardManager>
    {
        [Serializable]
        public class RewardSaveData
        {
            public int Gold => _gold;
            public int StarDust => _starDust;

            [SerializeField] private int _gold = 0;
            [SerializeField] private int _starDust = 0;

            public void AddGold(int amount) => _gold += amount;
            public void AddStarDust(int amount) => _starDust += amount;

            public bool ConsumeGold(int amount)
            {
                if (_gold < amount)
                {
                    return false;
                }

                _gold -= amount;
                return true;
            }

            public bool ConsumeStarDust(int amount)
            {
                if (_starDust < amount)
                {
                    return false;
                }

                _starDust -= amount;
                return true;
            }
        }

        public static event Action<RewardSaveData> OnRewardDataChanged;

        private RewardSaveData _currentData;
        private string _savePath;

        public RewardSaveData CurrentData => _currentData;

        protected override void Init()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "reward.dat");

            LoadData();
        }

        // ★ 보상 획득 (재화 증가)
        public void AddReward(int gold, int stardust)
        {
            _currentData.AddGold(gold);
            _currentData.AddStarDust(stardust);
            NotifyAndSave();
        }

        //재화 소비
        public bool ConsumeGold(int goldCost)
        {
            if (_currentData.Gold < goldCost)
            {
                return false; // 구매 실패
            }

            _currentData.ConsumeGold(goldCost);

            NotifyAndSave();
            return true;
        }

        public bool ConsumeStardust(int stardustCost)
        {
            if (_currentData.StarDust < stardustCost)
            {
                return false; // 구매 실패
            }

            _currentData.ConsumeStarDust(stardustCost);

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
                string json = JsonUtility.ToJson(_currentData);
                string encryptedJson = Encrypter.Encrypt(json);
                File.WriteAllText(_savePath, encryptedJson);
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
                    string encryptedJson = File.ReadAllText(_savePath);
                    string decryptedJson = Encrypter.Decrypt(encryptedJson);
                    _currentData = JsonUtility.FromJson<RewardSaveData>(decryptedJson);
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
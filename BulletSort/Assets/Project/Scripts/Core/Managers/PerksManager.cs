using Ingame.ExpSystem;
using Ingame.Perks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

struct RarityWeightRange
{
    int _min;
    int _max;

    public int Min { get { return _min; } }
    public int Max { get { return _max; } }

    public RarityWeightRange(int min, int max)
    {
        _min = min;
        _max = max;
    }
}

namespace Core
{
    class PerksManager : Singleton<PerksManager>
    {
        // UI는 이 이벤트를 구독하여 선택된 특전들을 띄운다.
        public event Action<int[]> OnPerksRolled;

        // 특전을 선택하였을 때 invoke되는 이벤트
        // 조건에 따라 특전 UI를 종료하거나 스테이지를 재개한다.
        public event Action OnPerkSelected;

        public event Action OnRerolled;

        public event Action OnPerkPhaseEnded;

        IReadOnlyDictionary<int, PerkData> _perksPool;
        Dictionary<int, List<PerkData>> _perksByRarity;

        IReadOnlyDictionary<int, RarityData> _rarityInfo;
        Dictionary<int, RarityWeightRange> _rarityWeightRangeInfo;
        int _maxWeight;

        HashSet<int> _perksSet;

        const int DEFAULT_REROLL_NUM = 1;
        int _remainRerollNum;
        public int RemainRerollNum { get { return _remainRerollNum; } }

        int _remainSelectNum;
        public int RemainSelectNum { get { return _remainSelectNum; } }

        const int MAX_PERKS_NUM = 3;
        int[] _perksSlot;

        private EffectManager _effectManager;


        protected override void Init()
        {
            //DataManager.Instance.GetData<PerksData>(...);
            _perksPool = DataManager.Instance.GetTable<PerkData>();

            // Rarity Info 초기화 & Weight 최대치 계산
            _rarityInfo = DataManager.Instance.GetTable<RarityData>();

            _rarityWeightRangeInfo = new Dictionary<int, RarityWeightRange>();

            _maxWeight = 0;
            foreach (var pair in _rarityInfo)
            {
                RarityData data = pair.Value;
                RarityWeightRange range = new RarityWeightRange(_maxWeight, _maxWeight + data.Weight);
                _rarityWeightRangeInfo.Add(pair.Key, range);
                _maxWeight += data.Weight;
            }

            // Rarity 기준으로 특전 정리
            _perksByRarity = new Dictionary<int, List<PerkData>>();

            foreach (var pair in _perksPool)
            {
                PerkData data = pair.Value;

                int rarity = data.PerkRarityType;

                if (!_perksByRarity.ContainsKey(rarity))
                {
                    _perksByRarity[rarity] = new List<PerkData>();
                }

                _perksByRarity[rarity].Add(data);
            }

            _perksSet = new HashSet<int>();

            _remainSelectNum = 0;

            ExpManager.OnLevelUp += LevelupHandler;
        }

        protected override void OnDestroy()
        {
            ExpManager.OnLevelUp -= LevelupHandler;
            base.OnDestroy();
        }

        private void LevelupHandler()
        {
            _remainSelectNum++;
        }

        public void InitState()
        {
            _remainRerollNum = DEFAULT_REROLL_NUM;
        }

        public void EnterPerksPhase()
        {
            if (_remainSelectNum <= 0)
            {
                return;
            }

            InitState();

            Time.timeScale = 0;
            ChoosePerks();
        }

        public void Reroll()
        {
            if (_remainRerollNum <= 0)
            {
                // 애초에 UI에서 비활성화 되어야 함.
                return;
            }

            _remainRerollNum--;

            OnRerolled();

            ChoosePerks();
        }


        private void ChoosePerks()
        {
            _perksSet.Clear();

            while (_perksSet.Count < MAX_PERKS_NUM)
            {
                while (true)
                {
                    int rarityID = RollRarityID();
                    int perkID = PickRandomPerkID(rarityID);
                    PerkData perk = _perksPool[perkID];

                    if (!_perksSet.Contains(perkID) && perk.CurLevel < perk.MaxLevel)
                    {
                        if (perk.CurLevel == perk.MaxLevel)
                        {
                            _perksByRarity[rarityID].Remove(perk);  // 최대 레벨을 달성한 특전은 pool에서 제거
                        }

                        _perksSet.Add(perkID);
                        break;
                    }
                }
            }
            // ...

            _perksSlot = _perksSet.ToArray();

            // UI 출력
            OnPerksRolled(_perksSlot);
        }

        private int RollRarityID()
        {
            int randVal = UnityEngine.Random.Range(0, _maxWeight);

            foreach (var pair in _rarityWeightRangeInfo)
            {
                RarityWeightRange range = pair.Value;

                if (range.Min <= randVal && randVal < range.Max)
                {
                    return pair.Key;
                }
            }

            Debug.LogWarning($"Random Value is Over Max Rarity Range, randVal : ${randVal}");
            return -1;
        }

        private int PickRandomPerkID(int rarityID)
        {
            int length = _perksByRarity[rarityID].Count;

            int randIndex = UnityEngine.Random.Range(0, length);

            int pickedId = _perksByRarity[rarityID][randIndex].PerkID;

            return pickedId;
        }

        public void SelectPerk(int index)
        {
            PerkData perk = _perksPool[_perksSlot[index]];
            perk.CurLevel++;

            _effectManager.ApplyEffect(perk.EffectID);

            Debug.Log($"<color=green>[PerksManager] : Perk {perk.PerkID} is Selected</color>");
            Debug.Log($"{perk.CurLevel - 1} → {perk.CurLevel}");

            _remainSelectNum--;

            if (_remainSelectNum == 0)
            {
                Time.timeScale = 1;
                OnPerkPhaseEnded();
                return;
            }
            else
            {
                OnPerkSelected();
            }

            ChoosePerks();
        }

        public void BindEffectManager(EffectManager manager)
        {
            _effectManager = manager;
        }
    }
}
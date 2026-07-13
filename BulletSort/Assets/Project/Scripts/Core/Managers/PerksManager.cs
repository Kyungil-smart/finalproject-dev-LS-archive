using Ingame.ExpSystem;
using Ingame.Perks;
using InGame.Slot;
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

        Dictionary<int, PerkData> _perksPool;
        Dictionary<int, List<PerkData>> _perksByRarity;

        // PerkID → 현재 레벨. SO(PerkData) 대신 런타임에서 관리.
        // Bind마다 새로 할당 → 런 시작 시 자동 초기화(에셋 오염·리셋 누락 문제 원천 제거).
        Dictionary<int, int> _perkLevel;

        IReadOnlyDictionary<int, RarityData> _rarityInfo;
        Dictionary<int, RarityWeightRange> _rarityWeightRangeInfo;
        int _maxWeight;

        HashSet<int> _perksSet;

        const int DEFAULT_REROLL_NUM = 1;
        int _remainRerollNum;
        public int RemainRerollNum { get { return _remainRerollNum; } }

        int _remainSelectNum;
        public int RemainSelectNum { get { return _remainSelectNum; } }

        int _totalSelectNum;
        public int TotalSelectNum { get { return _totalSelectNum; } }


        const int MAX_PERKS_NUM = 3;

        // 특전 시작 레벨. partial의 기존 CurLevel 초기값(0)과 일치.
        const int PERK_START_LEVEL = 0;

        // 룰렛 시도 상한(초과 시 결정적 폴백으로 마무리)
        const int MAX_ROLL_ATTEMPTS = 256;

        int[] _perksSlot;

        private EffectManager _effectManager;
        private SlotBoardManager _slotBoardManager;

        protected override void Init()
        {
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
            Debug.Log($"현 특전 선택 횟수 : {_remainSelectNum}");
        }

        public void InitState()
        {
            _totalSelectNum = _remainSelectNum;
            _remainRerollNum = DEFAULT_REROLL_NUM;
        }

        public void EnterPerksPhase()
        {
            if (_remainSelectNum <= 0)
            {
                OnPerkPhaseEnded?.Invoke();
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

            OnRerolled?.Invoke();

            ChoosePerks();
        }


        private void ChoosePerks()
        {
            _perksSet.Clear();

            // 레벨업 여지가 있는 특전 수 집계
            int selectableCount = CountSelectablePerks();

            // 뽑을 특전이 하나도 없으면 빈 창으로 멈추지 않고 페이즈 정상 종료
            if (selectableCount <= 0)
            {
                EndPerksPhase();
                return;
            }

            // 3개 미만이면 있는 만큼만 제시 → 무한 루프 원천 차단
            int targetCount = Mathf.Min(MAX_PERKS_NUM, selectableCount);

            RollPerksWeighted(targetCount);

            _perksSlot = _perksSet.ToArray();

            // UI 출력
            OnPerksRolled?.Invoke(_perksSlot);
        }

        // 레벨업 여지가 있는(중복 없는) 특전의 총 개수
        private int CountSelectablePerks()
        {
            int count = 0;

            foreach (var pair in _perksPool)
            {
                if (GetPerkLevel(pair.Key) < pair.Value.MaxLevel)
                {
                    count++;
                }
            }

            return count;
        }

        // 가중치 룰렛으로 targetCount개 뽑기.
        // 시도 상한을 두고, 못 채우면 결정적 폴백으로 마무리 → 항상 종료 보장.
        private void RollPerksWeighted(int targetCount)
        {
            int attempts = 0;

            while (_perksSet.Count < targetCount && attempts < MAX_ROLL_ATTEMPTS)
            {
                attempts++;

                int rarityID = RollRarityID();
                int perkID = PickRandomPerkID(rarityID);

                if (perkID == -1) continue;
                if (_perksSet.Contains(perkID)) continue;

                PerkData perk = _perksPool[perkID];
                if (GetPerkLevel(perkID) >= perk.MaxLevel) continue;

                _perksSet.Add(perkID);
            }

            // 룰렛이 시도 한계 내에 못 채운 잔여분을 결정적으로 채움(최후의 안전장치)
            if (_perksSet.Count < targetCount)
            {
                FillRemainingDeterministic(targetCount);
            }
        }

        // 남은 슬롯을 풀 전체 순회로 결정적으로 채움(가중치 무시)
        private void FillRemainingDeterministic(int targetCount)
        {
            foreach (var pair in _perksPool)
            {
                if (_perksSet.Count >= targetCount) break;

                if (GetPerkLevel(pair.Key) >= pair.Value.MaxLevel) continue;
                if (_perksSet.Contains(pair.Key)) continue;

                _perksSet.Add(pair.Key);
            }
        }

        // 특전 페이즈 정상 종료(시간 복구 + 종료 이벤트)
        private void EndPerksPhase()
        {
            Time.timeScale = 1;
            OnPerkPhaseEnded?.Invoke();
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
            if (!_perksByRarity.ContainsKey(rarityID))
            {
                return -1;
            }

            int length = _perksByRarity[rarityID].Count;

            if (length == 0)
            {
                return -1;
            }

            int randIndex = UnityEngine.Random.Range(0, length);

            int pickedId = _perksByRarity[rarityID][randIndex].PerkID;

            return pickedId;
        }

        public void SelectPerk(int index)
        {
            int perkID = _perksSlot[index];
            PerkData perk = _perksPool[perkID];

            int newLevel = GetPerkLevel(perkID) + 1;
            _perkLevel[perkID] = newLevel;

            // 만렙 도달 시 rarity 풀에서 제거(다음 룰렛에서 배제)
            if (newLevel >= perk.MaxLevel)
            {
                if (_perksByRarity.TryGetValue(perk.PerkRarityType, out var list))
                {
                    list.Remove(perk);
                }
            }

            _effectManager.ApplyEffect(perk.EffectID);

            Debug.Log($"<color=green>[PerksManager] : Perk {perk.PerkID} is Selected</color>");
            Debug.Log($"{newLevel - 1} → {newLevel}");

            _remainSelectNum--;
            _remainRerollNum = DEFAULT_REROLL_NUM;

            if (_remainSelectNum == 0)
            {
                EndPerksPhase();
                return;
            }
            else
            {
                OnPerkSelected?.Invoke();
            }

            ChoosePerks();
        }

        // UI 등 외부에서 특전 현재 레벨을 조회할 때 사용.
        public int GetPerkLevel(int perkID)
        {
            return _perkLevel != null && _perkLevel.TryGetValue(perkID, out int lv)
                ? lv
                : PERK_START_LEVEL;
        }

        public void BindSlotBoardManager(SlotBoardManager manager)
        {
            _slotBoardManager = manager;
            var towerTypes = _slotBoardManager.GetActiveTowerTypes();

            _perksPool = new Dictionary<int, PerkData>();

            var perksPoolOrigin = DataManager.Instance.GetTable<PerkData>();

            foreach (var perk in perksPoolOrigin)
            {
                PerkData data = perk.Value;

                if (data.IsActive == false)
                {
                    continue;
                }

                if (data.PerkTarget == 7 || data.PerkTarget == 8)
                {
                    _perksPool.Add(perk.Key, data);
                    continue;
                }

                foreach (int type in towerTypes)
                {
                    if (data.PerkTarget == type)
                    {
                        _perksPool.Add(perk.Key, data);
                        break;
                    }
                }
            }

            // 런타임 레벨 테이블 새로 생성 → 런 시작마다 시작 레벨로 초기화.
            _perkLevel = new Dictionary<int, int>(_perksPool.Count);
            foreach (var pair in _perksPool)
            {
                _perkLevel[pair.Key] = PERK_START_LEVEL;
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
        }

        public void BindEffectManager(EffectManager manager)
        {
            _effectManager = manager;
        }
    }
}
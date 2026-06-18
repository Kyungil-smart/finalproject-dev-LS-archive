using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 임시 Data 클래스
class PerkData : ScriptableObject
{
    public int PerkRarityType;
}
class EffectData : ScriptableObject
{
}

class RarityData : ScriptableObject
{
    public int Weight;
}

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
    class PerksManager : Singleton<StageManager>
    {
        IReadOnlyDictionary<int, PerkData> _perksPool;
        Dictionary<int, List<PerkData>> _perksByRarity;

        IReadOnlyDictionary<int, RarityData> _rarityInfo;
        Dictionary<int, RarityWeightRange> _rarityWeightRangeInfo;
        int _maxWeight;

        HashSet<int> _perksSet;

        const int DEFAULT_REROLL_NUM = 1;
        int _rerollCnt;

        const int MAX_PERKS_NUM = 3;
        int[] _perksSlot;

        protected override void Init()
        {
            //DataManager.Instance.GetData<PerksData>(...);
            _perksPool = DataManager.Instance.GetTable<PerkData>();

            _rerollCnt = DEFAULT_REROLL_NUM;

            // Rarity Info 초기화 & Weight 최대치 계산
            _rarityInfo = DataManager.Instance.GetTable<RarityData>();

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
        }

        public void EnterPerksPhase()
        {
            ChoosePerks();
        }

        private void Reroll()
        {
            if (_rerollCnt <= 0)
            {
                // 애초에 UI에서 비활성화 되어야 함.
                return;
            }

            _rerollCnt--;

            ChoosePerks();
        }


        private void ChoosePerks()
        {
            _perksSet.Clear();

            while (_perksSet.Count < MAX_PERKS_NUM)
            {
                int rarityID = RollRarityID();

                while (true)
                {
                    int perkID = PickRandomPerkID(rarityID);

                    if (!_perksSet.Contains(perkID))
                    {
                        _perksSet.Add(perkID);
                        break;
                    }
                }
            }
            // ...

            _perksSlot = _perksSet.ToArray();

            // UI 출력
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
            return -1;
        }
    }
}
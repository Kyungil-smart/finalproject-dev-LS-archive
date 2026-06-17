using System.Collections.Generic;

namespace InGame.Slot
{
    // 빈 슬롯 보충 시 어떤 기물을 넣을지 우선순위 선정
    // 1번째: 보드 전체 동일 기물 2개 → 3개 → 대기 그룹 잔량 가중 랜덤.
    // 2번째: 1번째를 보드 카운트에 +1 반영해 재탐색 — 다른 2개짜리가 있으면 그쪽을 뽑아
    //   같은 슬롯 동일 기물을 줄임. 단 대안이 없으면 같은 종류가 또 뽑힐 수 있음(최소화지 방지 아님).
    // 우선순위 1·2(2개·3개)는 조건 매칭, 미해당 시 대기 그룹 잔량 가중 랜덤.
    // 순수 C# — 보드 카운트·대기 그룹을 입력으로 받아 선정만. 보드 순회·집계는 매니저가.
    // 작성자: 이성규
    public static class PieceSelector
    {
        // 빈 슬롯에 넣을 기물 count개 선정. 반환된 ID는 호출부가 대기 그룹에서 제거.
        // boardCounts: 현재 보드 전체 기물 ID별 개수(매니저 1회 집계). 내부에서 복제본 수정.
        // waitingGroup: 대기 그룹 — 우선순위 후보는 여기 실제 남은 ID만 유효, 랜덤 출처이기도.
        public static List<int> Select(
            IReadOnlyDictionary<int, int> boardCounts,
            IReadOnlyList<int> waitingGroup,
            int count)
        {
            var result = new List<int>(count);
            
            // 보드 카운트 복제 — 1번째 선정 후 +1 반영해 2번째 재탐색에 씀.
            var counts = new Dictionary<int, int>(boardCounts.Count);
            foreach (var kv in boardCounts) counts[kv.Key] = kv.Value;
            
            // 대기 그룹 종류별 잔량 — 우선순위 후보 유효성(보드 2개여도 대기에 없으면 못 꺼냄) + 랜덤 차감용.
            var available = new Dictionary<int, int>();
            foreach (int id in waitingGroup)
                available[id] = available.TryGetValue(id, out int c) ? c + 1 : 1;

            for (int n = 0; n < count; n++)
            {
                int picked = PickOne(counts, available);
                if (picked == 0) break;  // 대기 그룹 소진 — 가능한 만큼만

                result.Add(picked);
                available[picked]--;  // 뽑은 만큼 대기 잔량 차감(다음 선정·랜덤이 중복 안 보게)

                // 방금 뽑은 기물을 보드 카운트에 +1 → 다음 선정이 포함해 재탐색.
                counts[picked] = counts.TryGetValue(picked, out int bc) ? bc + 1 : 1;
            }
            
            return result;
        }
        
        // 1개 선정 — 우선순위: 보드 2개 → 3개 → 랜덤. 후보는 대기 잔량 있는 종류로 한정.
        private static int PickOne(Dictionary<int, int> counts, Dictionary<int, int> available)
        {
            int byTwo = FindByBoardCount(counts, available, 2);
            if (byTwo != 0) return byTwo;

            int byThree = FindByBoardCount(counts, available, 3);
            if (byThree != 0) return byThree;

            return PickRandom(available);
        }
        
        // 보드에 정확히 target개 있는 종류 중, 대기 그룹에 남은 것 하나.
        private static int FindByBoardCount(
            Dictionary<int, int> counts, Dictionary<int, int> available, int target)
        {
            foreach (var kv in counts)
            {
                if (kv.Value != target) continue;
                if (available.TryGetValue(kv.Key, out int left) && left > 0)
                    return kv.Key;
            }
            return 0;
        }
        
        // 대기 그룹 잔량 있는 종류 중 랜덤 — 잔량 가중(많이 남은 종류가 더 자주).
        private static int PickRandom(Dictionary<int, int> available)
        {
            int total = 0;
            foreach (var kv in available)
                if (kv.Value > 0)
                    total += kv.Value;
            if(total == 0) return 0;
            
            int r = UnityEngine.Random.Range(0, total);
            foreach (var kv in available)
            {
                if (kv.Value <= 0) continue;
                if (r < kv.Value) return kv.Key;
                r -= kv.Value;
            }
            
            return 0; // 도달 안 함
        }
    }
}

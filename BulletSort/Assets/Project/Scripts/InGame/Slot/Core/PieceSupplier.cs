using System.Collections.Generic;
using System.Linq;
using Core;
using InGame.Sort;
using UnityEngine;

namespace InGame.Slot
{
    // 기물 공급기 — 대기 그룹 보유 + 슬롯 보충 + 재생성.
    // "무엇을 어떻게 채울지" 담당. "언제 채울지·재생성할지"는 매니저가 판단해 호출.
    // 작성자: 이성규
    public class PieceSupplier
    {
        // 슬롯에 보충할 기물 ID를 숫자로 들고 관리하는 풀.
        private readonly List<int> _waitingGroup = new List<int>();
        
        // 대기 그룹에 채울 기물 ID 목록 — DB에서 받음
        // enum이나 하드코딩 대신 데이터가 정한 ID로 채우기 위함
        private IReadOnlyList<int> _pieceIDs;
        
        // 대기 그룹 비었는지 — 매니저가 재생성 조건(대기 0 + 슬롯 0) 판단에 사용.
        public bool IsEmpty => _waitingGroup.Count == 0;
        
        // 명시적 초기화 — 매니저 Awake에서 호출. 채울 ID 목록(DB 제공)을 받아 보관 후 생성.
        public void Initialize(IReadOnlyList<int> pieceIDs)
        {
            _pieceIDs = pieceIDs;
            // 생성 시점을 매니저가 제어.
            Regenerate();
        }
        
        // 슬롯 보충 — 종류는 PieceSelector 우선순위로, 칸 위치는 빈 칸 셔플 랜덤.
        //   (정렬이 슬롯 내 동일 3개 판정이라 칸 위치·순서 무관 → 칸은 우선순위 불필요)
        // 정원 못 채우는 상황(대기 부족·빈 칸 부족)이면 아예 채우지 않음 — 1개짜리 슬롯 방지.
        // boardCounts: 매니저가 집계한 현재 보드 전체 기물 카운트
        public void RefillSlot(Slot slot, IReadOnlyDictionary<int, int> boardCounts)
        {
            var emptyCells = slot.GetEmptyCellIndices();
            
            // 슬롯당 정원이 원칙 — 정원 못 채울 상황이면 보충 보류.
            if (_waitingGroup.Count < Define.REFILL_PER_SLOT || emptyCells.Count < Define.REFILL_PER_SLOT) return;

            var picked = PieceSelector.Select(boardCounts, _waitingGroup, Define.REFILL_PER_SLOT);
            if (picked.Count < Define.REFILL_PER_SLOT) return;  // 못 채우면 보류(1개짜리 슬롯 방지)
            
            Shuffle(emptyCells);
            for (int i = 0; i < Define.REFILL_PER_SLOT; i++)
            {
                RemoveFromWaiting(picked[i]);
                slot.PlacePiece(emptyCells[i], picked[i]);
            }
        }
        
        // 대기 그룹에서 특정 ID 1개 제거
        private void RemoveFromWaiting(int pieceID)
        {
            int idx = _waitingGroup.IndexOf(pieceID);
            if (idx >= 0) _waitingGroup.RemoveAt(idx);
        }
        
        // 대기 그룹 재생성 — 호출 판단(보드 전체 클리어 여부)은 매니저가.
        // DB가 준 ID 목록 × PIECE_PER_TYPE개로 채움. ID 체계(8001 등)는 데이터가 정함.
        // 종류당 수량은 아직 상수(PIECE_PER_TYPE) — 정식 데이터 도입 시 데이터에서 읽도록 교체 가능.
        public void Regenerate()
        {
            _waitingGroup.Clear();
            
            foreach (int id in _pieceIDs)
            {
                for (int i = 0; i < Define.PIECE_PER_TYPE; i++)
                    _waitingGroup.Add(id);
            }
            
            // 종류별로 뭉쳐 들어간 걸 섞어 Dequeue 시 골고루 나오게.
            Shuffle(_waitingGroup);
        }
        
        // 리스트 인덱스 셔플 (Fisher-Yates) — 채울 칸을 랜덤 선정.
        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        
        // 디버그용 — 대기 그룹 상태 문자열.
        public string GetDebugInfo()
        {
            var counts = new Dictionary<int, int>();
            foreach (int id in _waitingGroup)
                counts[id] = counts.GetValueOrDefault(id, 0) + 1;
            
            var sb = new System.Text.StringBuilder($"대기 그룹: {_waitingGroup.Count}개");
            if (counts.Count > 0)
            {
                sb.Append(" (");
                // Key(ID) 기준으로 오름차순 정렬하여 출력합니다.
                foreach (var kv in counts.OrderBy(kv => kv.Key))
                    sb.Append($"ID{kv.Key}×{kv.Value} ");
                sb.Append(")");
            }
            return sb.ToString();
        }
    }
}
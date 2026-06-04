using System.Collections.Generic;
using System.Linq;
using Core;
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
        
        // 대기 그룹 비었는지 — 매니저가 재생성 조건(대기 0 + 슬롯 0) 판단에 사용.
        public bool IsEmpty => _waitingGroup.Count == 0;
        
        // 명시적 초기화 — 매니저 Awake에서 호출. 생성 시점을 매니저가 제어.
        public void Initialize() => Regenerate();
        
        // 슬롯에 기물 2개 배치 — 빈 칸 중 2칸 셔플 선정 후 대기 그룹에서 꺼내 채움.
        // 2개를 못 채우는 상황(대기 부족·빈 칸 부족)이면 아예 채우지 않음 — 1개짜리 슬롯 방지.
        public void RefillSlot(Slot slot)
        {
            var emptyCells = slot.GetEmptyCellIndices();
            
            // 슬롯당 2개가 원칙 — 2개 못 채울 상황이면 보충 보류.
            if (_waitingGroup.Count < Define.REFILL_PER_SLOT || emptyCells.Count < Define.REFILL_PER_SLOT) return;
            
            Shuffle(emptyCells);
            for (int i = 0; i < Define.REFILL_PER_SLOT; i++)
                slot.PlacePiece(emptyCells[i], Dequeue());
        }
        
        // 대기 그룹 재생성 — 호출 판단(보드 전체 클리어 여부)은 매니저가.
        // TODO(데이터 SO 도입 후) — 종류·수량을 데이터에서 읽어 채움.
        // 현재 데모: 6종(ID 1~6) × 9개씩 = 54개. 기획서 4-3절(3 × 6 × 3 = 54)과 정합.
        public void Regenerate()
        {
            _waitingGroup.Clear();
            for (int id = 1; id <= Define.PIECE_TYPE_COUNT; id++)
            { 
                for (int i = 0; i < Define.PIECE_PER_TYPE; i++) 
                    _waitingGroup.Add(id);
            }

            Shuffle(_waitingGroup);
        }
        
        // 대기 그룹 앞에서 기물 ID 1개 꺼냄.
        // TODO(PieceSelector 도입 후) — 단순 Dequeue를 우선순위 선정으로 교체.
        private int Dequeue()
        {
            int pieceID = _waitingGroup[0];
            _waitingGroup.RemoveAt(0);
            return pieceID;
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
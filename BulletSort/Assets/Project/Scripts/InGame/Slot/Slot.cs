using Core;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 1개. 셀 3개의 상태를 SGrid1D로 보유하고 정렬 판정·셀 조작을 담당.
    // 자식 SlotCell들은 표현(콜라이더·Pivot), 데이터는 이 클래스의 _cells에 집중.
    // 단방향 동기화 — 데이터(_cells)를 먼저 바꾸고 자식 SlotCell에 위치 반영 요청.
    // 작성자: 이성규
    public class Slot : MonoBehaviour
    {
        // 셀 3개의 런타임 상태. 인덱스 0~2로 접근.
        // 매치-3에서 가져온 SGrid1D<T>로 보드 표현(2차원은 오버스펙이라 1차원 개조).
        private SGrid1D<CellRuntimeData> _cells;
        
        void Awake()
        {
            // 슬롯당 셀 수 = 정렬 완성에 필요한 기물 수(SORT_COUNT) — 같은 의미라 상수 공유.
            _cells = new SGrid1D<CellRuntimeData>(Define.SORT_COUNT);
        }
    
        // 지정 셀이 빈 칸인지 — 드롭 판정 시 SlotCell이 위임 호출.
        public bool IsCellEmpty(int cellIndex) => _cells[cellIndex].IsEmpty;
    
        // 지정 셀에 기물 배치. 슬롯이 셀 상태를 단독 관리하므로 외부는 이 메서드로만 변경.
        public void PlacePiece(int cellIndex, int pieceID)
        {
            _cells[cellIndex] = new CellRuntimeData { PieceID = pieceID };
        }
    
        // 지정 셀 비우기 — 정렬 성공 후 셀 초기화·기물 이동 시 사용.
        public void ClearCell(int cellIndex)
        {
            _cells[cellIndex] = CellRuntimeData.Empty;
        }
    }
}
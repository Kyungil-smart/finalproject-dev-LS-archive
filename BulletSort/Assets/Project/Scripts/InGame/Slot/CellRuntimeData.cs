using UnityEngine;

namespace InGame.Slot
{
    // 셀 1칸의 런타임 데이터. 기획서 CellRuntimeData에 대응
    // 정적 데이터 (PieceData)는 PieceID 조회해서 사용
    // SGrod1D<CellRunTimeData>의 T로 들어가 Slot의 셀 상태를 표현한다.
    // 작성자: 이성규
    public struct CellRuntimeData
    {
        // 기물 ID,  0이면 빈 칸, 양수면 PieceData.PieceID
        // 0을 빈 칸 예약값으로 약속(데이터 담당과 합의 필요)
        public int PieceID;
        
        // 빈칸 여부
        public bool IsEmpty => PieceID == 0;
        
        // 빈 칸 상수. 코드에서 명시적으로 표현할 때 사용.
        public static CellRuntimeData Empty => new CellRuntimeData { PieceID = 0 };
    }
}
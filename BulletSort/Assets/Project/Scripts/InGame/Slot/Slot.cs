using System;
using Core;
using InGame.Sort;
using UnityEngine;
using Logger = Core.Logger;

namespace InGame.Slot
{
    // 슬롯 1개. 셀 3개의 상태를 SGrid1D로 보유하고 정렬 판정·셀 조작을 담당.
    // 자식 SlotCell들은 표현(콜라이더·Pivot), 데이터는 이 클래스의 _cells에 집중.
    // 단방향 동기화 — 데이터(_cells)를 먼저 바꾸고 자식 SlotCell·Piece에 반영.
    // 작성자: 이성규
    public class Slot : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private int _slotID;
        
        // 셀 3개의 런타임 상태. 인덱스 0~2로 접근.
        // 매치-3에서 가져온 SGrid1D<T>로 보드 표현 (2차원은 오버스펙이라 1차원 개조).
        private SGrid1D<CellRuntimeData> _cells;
        
        // 자식 SlotCell 인덱스 캐싱 — cellIndex로 직접 접근.
        private SlotCell[] _slotCells;
        
        public int SlotID => _slotID;
        
        // 3-Sort 정렬 성공 이벤트 — (slotID, pieceID).
        // 외부(포탑 소환 시스템 등)가 구독해 후속 처리.
        public event Action<int, int> OnSortSuccess;
        
        private void Awake()
        {
            // 슬롯당 셀 수 = 정렬 완성에 필요한 기물 수(SORT_COUNT) — 같은 의미라 상수 공유.
            _cells = new SGrid1D<CellRuntimeData>(Define.SORT_COUNT);
            
            // 자식 SlotCell들을 cellIndex 기준으로 정렬 캐싱
            var cells = GetComponentsInChildren<SlotCell>();
            _slotCells = new SlotCell[Define.SORT_COUNT];
            foreach (var cell in cells)
                _slotCells[cell.CellIndex] = cell;
        }
        
        private void Start()
        {
            // 임시 초기화 — 자식 SlotCell의 자식에 Piece가 있으면 데이터 등록.
            // 정식 초기 배치(SlotBoardManager)가 들어오면 교체.
            for (int i = 0; i < Define.SORT_COUNT; i++)
            {
                var piece = _slotCells[i].GetComponentInChildren<Piece>();
                if (piece != null && piece.PieceID > 0) 
                    _cells[i] = new CellRuntimeData { PieceID = piece.PieceID };
            }
        }
        
        // 지정 셀이 빈 칸인지 — 드롭 판정 시 SlotCell이 위임 호출.
        public bool IsCellEmpty(int cellIndex) => _cells[cellIndex].IsEmpty;
        
        // 지정 셀에 기물 배치. 슬롯이 셀 상태를 단독 관리하므로 외부는 이 메서드로만 변경.
        public void PlacePiece(int cellIndex, int pieceID)
        {
            _cells[cellIndex] = new CellRuntimeData { PieceID = pieceID };
        }
        
        // 지정 셀 비우기 — 데이터 + 대응 Piece 비주얼 끔.
        public void ClearCell(int cellIndex)
        {
            _cells[cellIndex] = CellRuntimeData.Empty;
            
            // 비주얼 갱신 — 대응 Piece 끄기 (단방향 동기화)
            var piece = _slotCells[cellIndex].GetComponentInChildren<Piece>();
            if (piece != null)
                piece.gameObject.SetActive(false);
        }
        
        // 셀 3개 전체 비우기 — 정렬 성공 후 사용.
        public void ClearAllCells()
        {
            for (int i = 0; i < Define.SORT_COUNT; i++)
                ClearCell(i);
        }
        
        // 자기 셀 3개가 모두 같은 PieceID인지 (빈 칸 제외).
        public bool IsSorted()
        {
            int first = _cells[0].PieceID;
            if (first == 0) return false;  // 첫 칸이 빈 칸이면 정렬 불가
            
            // 첫 칸을 기준값으로 잡았으니 나머지 셀과 비교
            for (int i = 1; i < Define.SORT_COUNT; i++)
            {
                if (_cells[i].PieceID != first) return false;
            }
            return true;
        }
        
        // 정렬된 PieceID 반환 — 호출 전 IsSorted true 확인 약속.
        public int GetSortedPieceID() => _cells[0].PieceID;
        
        // 정렬 판정 + 성공 시 이벤트 발행 + 셀 비우기.
        // 드롭 성공 직후 호출되어 한 사이클(판정 → 외부 알림 → 정리)을 처리.
        public void CheckSort()
        {
            if (!IsSorted()) return;
            
            int sortedPieceID = GetSortedPieceID();
            Logger.Instance.LogInfo($"정렬 성공! SlotID={_slotID}, PieceID={sortedPieceID}");
            
            // 외부 발행 — 안정연 영역(포탑 소환)이 구독
            OnSortSuccess?.Invoke(_slotID, sortedPieceID);
            
            // 셀 3개 비우기 — 다음 정렬 사이클 준비
            ClearAllCells();
        }
        
        // 슬롯의 셀에 인덱스로 직접 접근.
        public SlotCell GetSlotCellByIndex(int cellIndex) => _slotCells[cellIndex];
    }
}
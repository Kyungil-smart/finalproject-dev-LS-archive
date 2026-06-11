using System;
using System.Collections.Generic;
using Core;
using InGame.Sort;
using UnityEngine;
using Logger = Core.Logger;

namespace InGame.Slot
{
    // 슬롯 1개. 셀 3개의 상태를 SGrid1D로 보유하고 정렬 판정·셀 조작을 담당.
    // 자식 SlotCell들은 표현(콜라이더·Pivot), 데이터는 이 클래스의 _cells에 집중.
    // 단방향 동기화 — 데이터(_cells)를 먼저 바꾸고 자식 SlotCell·Piece에 반영.
    // 전투 피격(HP)은 SlotHealth 컴포넌트로 분리 — Slot은 게임 로직(정렬·보충)만.
    // 작성자: 이성규
    public class Slot : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private int _slotID;

        [Header("Health")]
        [Tooltip("슬롯 체력 컴포넌트 — 전투 피격 처리. 비우면 Awake에서 탐색")]
        [SerializeField] private SlotHealth _slotHealth;
        
        // 셀 3개의 런타임 상태. 인덱스 0~2로 접근.
        // 매치-3에서 가져온 SGrid1D<T>로 보드 표현 (2차원은 오버스펙이라 1차원 개조).
        private SGrid1D<CellRuntimeData> _cells;
        
        // 자식 SlotCell 인덱스 캐싱 — cellIndex로 직접 접근.
        private SlotCell[] _slotCells;
        
        public int SlotID => _slotID;
        
        // 체력 컴포넌트 노출 — 외부가 slot.Health.TakeDamage()로 접근 가능.
        // (전투가 콜라이더로 IDamageable 직접 접근도 가능 — 양쪽 다 열어둠)
        public SlotHealth Health => _slotHealth;
        
        // 3-Sort 정렬 성공 이벤트 — (slotID, pieceID).
        // 외부(포탑 소환 시스템 등)가 구독해 후속 처리.
        public event Action<int, int> OnSortSuccess;
        
        // 셀 변경 이벤트 — Place/Clear 호출 시 발행.
        // 보드 관리자가 빈 칸 감지·보충 흐름에 사용.
        public event Action<Slot, int> OnCellChanged;  // (this, cellIndex)
        
        #region 유니티 라이프사이클
        
        private void Awake()
        {
            // 슬롯당 셀 수 = 정렬 완성에 필요한 기물 수(SORT_COUNT) — 같은 의미라 상수 공유.
            _cells = new SGrid1D<CellRuntimeData>(Define.SORT_COUNT);
            
            // SlotHealth 탐색 — 인스펙터 미지정 시. 루트에 함께 붙는 게 기본(피격 = 슬롯 단위),
            // 혹시 자식에 뒀어도 잡히도록 GetComponentInChildren로 fallback.
            if (_slotHealth == null)
                _slotHealth = GetComponent<SlotHealth>()
                              ?? GetComponentInChildren<SlotHealth>(includeInactive: true);
            
            // 자식 SlotCell들을 cellIndex 기준으로 정렬 캐싱
            var cells = GetComponentsInChildren<SlotCell>();
            _slotCells = new SlotCell[Define.SORT_COUNT];
            foreach (var cell in cells)
                _slotCells[cell.CellIndex] = cell;
        }
        
        #endregion
        
        #region 셀 조작 (단일 셀)
        
        // 지정 셀이 빈 칸인지 — 드롭 판정 시 SlotCell이 위임 호출.
        public bool IsCellEmpty(int cellIndex) => _cells[cellIndex].IsEmpty;
        
        // 지정 셀에 기물 배치. 슬롯이 셀 상태를 단독 관리하므로 외부는 이 메서드로만 변경.
        public void PlacePiece(int cellIndex, int pieceID)
        {
            _cells[cellIndex] = new CellRuntimeData { PieceID = pieceID };
            
            // 비주얼 갱신 — 풀링 Piece가 SetByID로 켜짐
            _slotCells[cellIndex].Piece?.SetByID(pieceID);

            OnCellChanged?.Invoke(this, cellIndex);
        }
        
        // 지정 셀 비우기 — 데이터 + 대응 Piece 비주얼 끔.
        public void ClearCell(int cellIndex)
        {
            _cells[cellIndex] = CellRuntimeData.Empty;
            
            // 비주얼 갱신 — 풀링 Piece가 SetByID(0)로 꺼짐
            _slotCells[cellIndex].Piece?.SetByID(0);

            OnCellChanged?.Invoke(this, cellIndex);
        }
        
        // 셀 3개 전체 비우기 — 정렬 성공 후 사용.
        public void ClearAllCells()
        {
            for (int i = 0; i < Define.SORT_COUNT; i++)
                ClearCell(i);
        }
        
        #endregion

        #region 슬롯 전체 조회

        // 빈 칸 인덱스 리스트 — 보충 시 어느 셀에 넣을지 결정
        public List<int> GetEmptyCellIndices()
        {
            var empties = new List<int>();
            for(int i=0; i < Define.SORT_COUNT; i++)
                if(_cells[i].IsEmpty) empties.Add(i);
            return empties;
        }
        
        // 슬롯의 모든 셀이 비어있는지 — 재생성 AND 조건 검사용
        public bool IsAllEmpty()
        {
            for (int i = 0; i < Define.SORT_COUNT; i++)
                if (!_cells[i].IsEmpty) return false;
            return true;
        }

        #endregion

        #region 정렬

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
            
            // 외부 발행 — 안정연 영역(포탑 소환)이 구독
            OnSortSuccess?.Invoke(_slotID, sortedPieceID);
            
            // 셀 3개 비우기 — 다음 정렬 사이클 준비
            ClearAllCells();
        }

        #endregion

        #region 접근자

        // 슬롯의 셀에 인덱스로 직접 접근.
        public SlotCell GetSlotCellByIndex(int cellIndex) => _slotCells[cellIndex];

        #endregion

        #region 디버그

        // 디버그용 — 셀 상태 문자열 (예: "Slot 0: [1][1][_]")
        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder($"Slot {_slotID}: ");
            for (int i = 0; i < Define.SORT_COUNT; i++)
            {
                int id = _cells[i].PieceID;
                sb.Append(id == 0 ? "[_]" : $"[{id}]");
            }
            return sb.ToString();
        }

        #endregion
    }
}
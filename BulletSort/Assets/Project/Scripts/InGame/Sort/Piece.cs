using Core;
using InGame.Slot;
using UnityEngine;
using Logger = Core.Logger;

namespace InGame.Sort
{
    // 임시 — 데이터 SO 도입 전까지 인스펙터 드롭다운 + 0 입력 방지용.
    // 정식 ID는 데이터 담당이 매김, 여기선 enum → ID 임시 매핑만.
    public enum PieceType
    {
        None,       // 0 풀링 비활성 — SetData 받기 전 / 빈 칸 표시
        Basic,      // 1
        Shotgun,    // 2
        LongRange,  // 3
        Tank,       // 4
        Splash,     // 5
        Support,    // 6
    }
    
    // 3-Sort 기물. 드래그 가능한 오브젝트.
    // 1차 구현: 손가락 따라가기 + 놓으면 원위치 복귀.
    // 슬롯 시스템 진입 후 SortResult 발행·셀 진입 등 게임 로직 추가 예정.
    // 작성자: 이성규
    public class Piece : MonoBehaviour, IDraggable
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private LayerMask _cellLayer;
        
        [Header("Piece Data (임시 — 데이터 파싱 SO 도입 후 SetData로 교체)")]
        [SerializeField] private PieceType _pieceType = PieceType.None;
        
        [Header("Sorting")]
        [Tooltip("드래그 중 기물이 올라갈 소팅 레이어")]
        [SerializeField] private SortingLayerType _draggingLayer = SortingLayerType.Dragging;
        
        
        // 드래그 사이클 동안만 유효한 캐시 묶음.
        // OnGrabbed에서 채우고 OnReleased까지 참조.
        private DragState _dragState;
        
        private struct DragState
        {
            public Vector3 OriginalPos;         // 드래그 시작 전 원래 위치 — 놓을 때 복귀 기준
            public Vector2 DragOffset;          // 터치 지점과 Piece 중심의 오프셋 — 잡은 지점에 자연스럽게 붙음
            public string OriginalSortingLayer; // 드래그 시작 시 백업 — 놓을 때 복귀
            public int OriginalSortingOrder;    // (같은 백업 묶음)
            public SlotCell OriginSlotCell;     // 출발 셀 — 데이터 이동 시 비우기 대상
        }
        
        // 외부(Slot 정렬 판정 등) 접근용 — 미래 SO 도입 시 내부 구현만 _data.PieceID로 교체.
        public int PieceID => _pieceType switch
        {
            PieceType.None  => 0,    // 빈 칸 예약값과 일치
            PieceType.Basic => 1,
            _ => 0,
        };
        
        void Awake()
        {
            if(_renderer == null) 
                _renderer = GetComponentInChildren<SpriteRenderer>();
            if(_collider == null) 
                _collider = GetComponent<Collider2D>();
        }
        
        // 풀링 재사용 시 호출. PieceID → PieceType 매핑 + 활성 토글.
        // TODO(데이터 SO 도입 후) — switch 매핑 폐기, _data 참조로 교체.
        public void SetByID(int pieceID)
        {
            _pieceType = pieceID switch
            {
                0 => PieceType.None,
                1 => PieceType.Basic,
                _ => PieceType.None,
            };
            
            gameObject.SetActive(_pieceType != PieceType.None);
        }
        
        public void OnGrabbed(Vector2 worldPos)
        {
            _dragState = new DragState
            {
                OriginalPos = transform.position,
                DragOffset = (Vector2)transform.position - worldPos,
                OriginalSortingLayer = _renderer.sortingLayerName,
                OriginalSortingOrder = _renderer.sortingOrder,
                OriginSlotCell = FindOriginCell()
            };
            
            // 소팅 백업 후 드래그 레이어로 올림 — 옆 슬롯 Frame·SlotUI 뒤로 안 숨음.
            _renderer.sortingLayerName = _draggingLayer.ToName();
            
            // 자기 자신이 OverlapPoint에 잡히는 사고 차단.
            _collider.enabled = false;
        }
        
        public void OnDragging(Vector2 worldPos)
        {
            // z는 원래 값 유지 — 카메라 거리에 영향 받지 않게.
            Vector2 newPos = worldPos + _dragState.DragOffset;
            transform.position = new Vector3(newPos.x, newPos.y, _dragState.OriginalPos.z);
        }
        
        public void OnReleased(Vector2 worldPos)
        {
            PlaceOnCell(FindCellAt(worldPos));
            
            // 원위치 복귀 (성공·실패 무관)
            // 성공 시: 자기는 이미 SetByID(0)로 꺼졌고, 도착 셀 Piece가 켜짐
            // 실패 시: 그대로 자리 유지
            transform.position = _dragState.OriginalPos;
            
            // 소팅·콜라이더 복귀.
            _renderer.sortingLayerName = _dragState.OriginalSortingLayer;
            _renderer.sortingOrder = _dragState.OriginalSortingOrder;
            _collider.enabled = true;
        }
        
        // 지정 좌표에서 Cell 레이어 콜라이더 검색 → SlotCell 반환 (없으면 null)
        private SlotCell FindCellAt(Vector2 worldPos)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos, _cellLayer);
            return hit != null ? hit.GetComponent<SlotCell>() : null;
        }
        
        // 잡힐 시점의 자기 위치 셀 찾기 — 출발 셀 캐싱용.
        private SlotCell FindOriginCell() => FindCellAt(transform.position);

        // 대상 셀에 데이터 이동 — Piece 자체는 자기 셀에 영구 매핑, 데이터만 이동.
        // 출발 셀이 SetByID(0)로 꺼지고, 도착 셀의 풀링 Piece가 SetByID로 켜지는 흐름.
        private void PlaceOnCell(SlotCell targetCell)
        {
            // 타겟 Cell이 없거나 비워져있지 않다면 배치 실패
            if (targetCell == null || !targetCell.IsEmpty)
                return;
            
            // 자기 PieceID 캐싱 — ClearCell이 자기 _pieceType을 None으로 바꿔버리기 전에
            int movingPieceID = PieceID;
            
            // 데이터만 이동 — 출발 비우기 → 도착 배치 순서
            if (_dragState.OriginSlotCell != null)
                _dragState.OriginSlotCell.Slot.ClearCell(_dragState.OriginSlotCell.CellIndex);
            
            targetCell.Slot.PlacePiece(targetCell.CellIndex, movingPieceID);
            
            // 정렬 판정 — 성공 시 Slot 내부에서 이벤트·셀 비우기 처리
            targetCell.Slot.CheckSort(); 
        }
    }
}
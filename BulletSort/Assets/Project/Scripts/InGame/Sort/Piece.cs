using Core;
using InGame.Slot;
using InGame.Sort.Data;
using UnityEngine;

namespace InGame.Sort
{
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
        
        [Header("Piece Data (임시 — 데이터 파싱 SO 도입 후 정식 DB로 교체)")]
        [Tooltip("기물 데이터 조회 DB — SetByID 시 PieceID로 스프라이트 조회. 프리팹 1개에 꽂으면 풀링 인스턴스 공유")]
        [SerializeField] private PieceDatabase _pieceDatabase;
        
        // 현재 기물 ID — 데이터 체계(8001 등) 그대로 보유. 0은 빈 칸 예약값.
        private int _pieceID;
        
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
        
        // 외부(Slot 정렬 판정 등) 접근용 — 보유한 ID 그대로 반환(데이터 체계 수용).
        public int PieceID => _pieceID;
        
        void Awake()
        {
            if(_renderer == null) 
                _renderer = GetComponentInChildren<SpriteRenderer>();
            if(_collider == null) 
                _collider = GetComponent<Collider2D>();
        }
        
        // 풀링 재사용 시 호출. PieceID 보유 + 스프라이트 교체 + 활성 토글
        // 0은 빈 칸 예약값이라 끔. 그 외엔 DB 조회로 스프라이트 갱신(유효성은 DB 등록 여부로 판단).
        public void SetByID(int pieceID)
        {
            _pieceID = pieceID;
            
            bool active = pieceID != 0;  // 0 = 빈 칸 예약값
            gameObject.SetActive(active);
            
            // 켜질 때만 스프라이트 교체 (0은 꺼지므로 갱신 불필요)
            if (active && _renderer != null && _pieceDatabase != null)
            {
                PieceData data = _pieceDatabase.GetByID(pieceID);
                if (data != null && data.Sprite != null)
                    _renderer.sprite = data.Sprite; 
            }
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

        // 대상 셀로 기물 데이터 이동 — Piece 자체는 자기 셀에 영구 매핑, 데이터만 옮김.
        // 도착 셀의 풀링 Piece가 켜지고, 출발 셀의 풀링 Piece가 꺼지는 흐름.
        private void PlaceOnCell(SlotCell targetCell)
        {
            // 타겟 셀이 없거나 비어있지 않으면 배치 실패
            if (targetCell == null || !targetCell.IsEmpty) return;
            
            // 옮길 PieceID 캐싱 — 아래 ClearCell이 자기 _pieceID를 0으로 바꾸기 전에 확보
            int movingPieceID = PieceID;
            
            // 도착 먼저 → 출발 나중 순서.
            // 같은 슬롯 안에서 옮길 때 '도착 비우고 출발 채우기' 사이 순간 전체 빔 상태를 막아
            // 그 틈에 보충 이벤트가 끼어드는 사고를 방지.
            targetCell.Slot.PlacePiece(targetCell.CellIndex, movingPieceID);
            
            if (_dragState.OriginSlotCell != null)
                _dragState.OriginSlotCell.Slot.ClearCell(_dragState.OriginSlotCell.CellIndex);
            
            // 정렬 판정 — 성공 시 Slot 내부에서 이벤트 발행·셀 비우기 처리
            targetCell.Slot.CheckSort(); 
        }
    }
}
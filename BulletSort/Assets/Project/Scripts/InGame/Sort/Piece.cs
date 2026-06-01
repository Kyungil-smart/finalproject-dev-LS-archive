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
        [SerializeField] private PieceType _pieceType = PieceType.Basic;
        
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
        
        public void OnGrabbed(Vector2 worldPos)
        {
            Logger.Instance.LogInfo($"{gameObject.name} OnGrabbed");
            
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
            Logger.Instance.LogInfo($"{gameObject.name} OnReleased");
            
            SlotCell targetCell = FindCellAt(worldPos);
            bool success = TryPlaceOnCell(targetCell);
            
            if (!success)
            {
                // 실패 — 원위치 복귀.
                transform.position = _dragState.OriginalPos;
            }
            
            // 소팅·콜라이더 복귀.
            _renderer.sortingLayerName = _dragState.OriginalSortingLayer;
            _renderer.sortingOrder = _dragState.OriginalSortingOrder;
            _collider.enabled = true;
            
            Logger.Instance.LogInfo($"드롭 {(success ? "성공" : "실패")}");
        }
        
        // 지정 좌표에서 Cell 레이어 콜라이더 검색 → SlotCell 반환 (없으면 null)
        private SlotCell FindCellAt(Vector2 worldPos)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos, _cellLayer);
            return hit != null ? hit.GetComponent<SlotCell>() : null;
        }
        
        // 잡힐 시점의 자기 위치 셀 찾기 — 출발 셀 캐싱용.
        private SlotCell FindOriginCell() => FindCellAt(transform.position);

        // 대상 셀에 배치 시도. 빈 셀이면 데이터 이동 + 비주얼 스냅 + 부모 재배치.
        // 반환: 성공 여부.
        private bool TryPlaceOnCell(SlotCell targetCell)
        {
            // 타겟 Cell이 없거나 비워져있지 않다면 배치 실패
            if (targetCell == null || !targetCell.IsEmpty)
                return false;

            // 데이터 이동 — 출발 비우기 → 도착 배치 순서.
            if (_dragState.OriginSlotCell != null)
                _dragState.OriginSlotCell.Slot.ClearCell(_dragState.OriginSlotCell.CellIndex);
            
            targetCell.Slot.PlacePiece(targetCell.CellIndex, PieceID);
            
            // 비주얼 — Cell_Pivot 자식으로 재배치 후 위치 스냅.
            transform.SetParent(targetCell.PivotTransform);
            transform.position = targetCell.PivotTransform.position;
            
            return true;
        }
        
        // TODO: 미래 SO 도입 시 활성. 풀링 재활용 시 호출되어 비주얼·정체성 갱신.
        // 현재는 임시 SerializeField _pieceID로 우회
        // public void SetData(PieceData data) { ... }
    }
}
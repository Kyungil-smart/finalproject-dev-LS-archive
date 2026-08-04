using InGame.Sort;
using UnityEngine;
using Logger = Core.Logger;

namespace InGame.Slot
{
    // 슬롯 안 셀 1칸. 드롭 판정 진입점 + 배치 기준점 노출.
    // 셀 상태(CellRuntimeData)는 부모 Slot의 SGrid1D가 보유 — 단방향 동기화.
    // 풀링 Piece는 셀에 영구 매핑 — Awake에서 캐싱.
    // 작성자: 이성규
    public class SlotCell : MonoBehaviour
    {
        [Header("인덱스")]
        [SerializeField] private int _cellIndex;          // 0~2 (슬롯 안 셀 위치)
        
        [Header("배치 기준점")]
        [SerializeField] private Transform _cellPivot;    // 기물이 놓일 월드 위치
        
        [Header("하이라이트")]
        [SerializeField] private SpriteRenderer _cellVisual;  // 색 교체 대상(Center_Visual 등)
        [SerializeField] private Color _highlightColor = new Color(1f, 0.92f, 0.3f, 0.7f);  // 반투명 노랑
        private Color _baseColor = Color.white;  // 프리팹 원본색, Awake에서 캐싱
        
        // 부모 Slot 참조 — 정렬 판정·셀 상태 조회 시 사용.
        private Slot _slot;
        
        // 풀링 Piece 캐싱 — 셀과 1:1 영구 매핑, 데이터 변경 시 SetByID로 갱신
        private Piece _piece;
        
        // 외부 접근용 프로퍼티
        public int CellIndex => _cellIndex;
        public Slot Slot => _slot;
        public Piece Piece => _piece;
        public Vector3 Position => _cellPivot.position;
        public Transform PivotTransform => _cellPivot;

        private void Awake()
        {
            _slot = GetComponentInParent<Slot>();
            if(_slot == null)
                Logger.Instance.LogError($"{gameObject.name}: 부모에 Slot이 없음");
            
            // 풀링 Piece 캐싱 — 비활성 포함
            _piece = GetComponentInChildren<Piece>(includeInactive: true);
            if (_piece == null)
                Logger.Instance.LogError($"{gameObject.name}: 자식에 Piece가 없음");
            
            // 하이라이트 비주얼 — 미지정 시 자식에서 탐색. 기본색은 프리팹 현재값 캐싱.
            if (_cellVisual == null)
                _cellVisual = GetComponentInChildren<SpriteRenderer>(includeInactive: true);
            if (_cellVisual != null)
                _baseColor = _cellVisual.color;
        }
        
        // 하이라이트 토글 — 드래그 중 배치 가능 호버 시 ON(색 교체), 벗어나면 OFF(기본색 복귀).
        //   색만 바꿈(테두리 별도 오브젝트 없음) — 파괴 슬롯 DestroyedUnderlay와 겹쳐도 무난(기획 3.4).
        public void SetHighlight(bool on)
        {
            if (_cellVisual == null) return;
            _cellVisual.color = on ? _highlightColor : _baseColor;
        }
        
        // 이 셀이 빈칸인지 — Slot의 SGrid1D에서 조회.
        public bool IsEmpty => _slot != null && _slot.IsCellEmpty(_cellIndex);
        
        // 임시 디버그용 — 검증 후 제거
        [ContextMenu("Debug Info")]
        private void DebugInfo()
        {
            var logText = (
                $"{gameObject.name} — CellIndex:{_cellIndex} " +
                $"Slot:{(_slot != null ? _slot.name : "NULL")} " +
                $"Pivot:{(_cellPivot != null ? _cellPivot.position.ToString() : "NULL")} " +
                $"IsEmpty:{IsEmpty}"
            );

            Logger.Instance.LogInfo(logText);
            Debug.Log(logText);
        }
    }
}

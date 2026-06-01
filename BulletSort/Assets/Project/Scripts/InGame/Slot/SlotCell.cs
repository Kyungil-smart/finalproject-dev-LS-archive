using System;
using UnityEngine;
using Logger = Core.Logger;

namespace InGame.Slot
{
    // 슬롯 안 셀 1칸. 드롭 판정 진입점 + 배치 기준점 노출.
    // 셀 상태(CellRuntimeData)는 부모 Slot의 SGrid1D가 보유 — 단방향 동기화.
    // 작성자: 이성규
    public class SlotCell : MonoBehaviour
    {
        [Header("인덱스")]
        [SerializeField] private int _cellIndex;          // 0~2 (슬롯 안 셀 위치)
        
        [Header("배치 기준점")]
        [SerializeField] private Transform _cellPivot;    // 기물이 놓일 월드 위치
        
        // 부모 Slot 참조 — 정렬 판정·셀 상태 조회 시 사용.
        private Slot _slot;
        
        // 외부 접근용 프로퍼티
        public int CellIndex => _cellIndex;
        public Slot Slot => _slot;
        public Vector3 Position => _cellPivot.position;
        public Transform PivotTransform => _cellPivot;

        private void Awake()
        {
            _slot = GetComponentInParent<Slot>();
            if(_slot == null)
                Logger.Instance.LogError($"{gameObject.name}: 부모에 Slot이 없음");
        }
        
        // 이 셀이 빈칸인지 — Slot의 SGrid1D에서 조회.
        public bool IsEmpty => _slot != null && _slot.IsCellEmpty(_cellIndex);
        
        // SlotCell.cs 임시 추가 (확인용, 검증 후 제거)
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

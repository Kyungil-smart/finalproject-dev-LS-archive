using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Logger = Core.Logger;

namespace InGame.Slot
{
    // 슬롯 9개와 대기 그룹을 통합 관리하는 보드 매니저.
    // - 초기 배치
    // - 슬롯 이벤트 구독 → 정렬 성공·셀 변경 응답
    // - 빈 칸 보충 흐름 + 재생성 AND 조건
    // - 외부에 정렬 성공 이벤트 통합 발행
    // 작성자: 이성규
    public class SlotBoardManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("자식으로 둔 슬롯 9개를 SlotID 순서로 등록")]
        [SerializeField] private List<Slot> _slots;
        
        // 정렬 성공 통합 발행 — 포탑 소환등 외부 구독
        public event Action<int, int> OnSortSuccess;  // (slotID, pieceID)
        
        // TODO(WaitingGroup 도입 후) — 임시 대기 그룹. 지금은 단순 리스트.
        private List<int> _waitingGroup = new List<int>();
        
        #region 유니티 라이프사이클
        
        private void Awake()
        {
            ValidateSlots();
        }
        
        private void Start()
        {
            SubscribeSlotEvents();
            InitialPlacement();
        }
        
        private void OnDestroy()
        {
            UnsubscribeSlotEvents();
        }
        
        #endregion

        #region 초기 배치

        private void InitialPlacement()
        {
            PrepareWaitingGroup();

            foreach (var slot in _slots)
            {
                PlaceInitialPiecesInSlot(slot);
            }
        }

        private void PrepareWaitingGroup()
        {
            // TODO(데이터 SO 도입 후) — 기획서 4-3절 종류별 9개씩
            // 데모: Basic(1) 충분량
            for (int i = 0; i < 50; i++)
                _waitingGroup.Add(1);
        }

        private void PlaceInitialPiecesInSlot(Slot slot)
        {
            var emptyCells = slot.GetEmptyCellIndices();
            Shuffle(emptyCells);
    
            int placeCount = Mathf.Min(2, emptyCells.Count, _waitingGroup.Count);
            for (int i = 0; i < placeCount; i++)
            {
                int pieceID = DequeueFromWaitingGroup();
                slot.PlacePiece(emptyCells[i], pieceID);
            }
        }

        private int DequeueFromWaitingGroup()
        {
            if (_waitingGroup.Count == 0) return 0;
            int pieceID = _waitingGroup[0];
            _waitingGroup.RemoveAt(0);
            return pieceID;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        #endregion
        
        #region 슬롯 셋업 검증
        
        private void ValidateSlots()
        {
            if (_slots == null || _slots.Count == 0)
            {
                Logger.Instance.LogError("SlotBoardManager — 슬롯 리스트 비어있음");
                return;
            }
            
            // SlotID와 인덱스가 일치하는지 검증 (인스펙터 순서 사고 방지)
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] == null)
                {
                    Logger.Instance.LogError($"SlotBoardManager — _slots[{i}] null");
                    continue;
                }
                
                if (_slots[i].SlotID != i)
                    Logger.Instance.LogWarning(
                        $"SlotBoardManager — _slots[{i}]의 SlotID={_slots[i].SlotID} (인덱스 불일치)");
            }
        }
        
        #endregion
        
        #region 슬롯 이벤트 구독·해제
        
        private void SubscribeSlotEvents()
        {
            foreach (var slot in _slots)
            {
                if (slot == null) continue;
                slot.OnSortSuccess += HandleSlotSorted;
                slot.OnCellChanged += HandleCellChanged;
            }
        }
        
        private void UnsubscribeSlotEvents()
        {
            foreach (var slot in _slots)
            {
                if (slot == null) continue;
                slot.OnSortSuccess -= HandleSlotSorted;
                slot.OnCellChanged -= HandleCellChanged;
            }
        }
        
        #endregion
        
        #region 슬롯 이벤트 응답
        
        // 슬롯 정렬 성공 응답 — 외부 통합 발행 + 빈 칸 보충
        private void HandleSlotSorted(int slotID, int pieceID)
        {
            Logger.Instance.LogInfo($"보드 매니저 — 슬롯 {slotID} 정렬 성공, PieceID={pieceID}");
            
            // 외부 발행 — 외부 영역(포탑 소환)이 구독
            OnSortSuccess?.Invoke(slotID, pieceID);
            
            // TODO — 정렬 성공 슬롯의 빈 칸 보충
            // RefillSlot(GetSlotByID(slotID));
        }
        
        // 슬롯 셀 변경 응답 — 드래그 이동 등으로 빈 칸 발생 시 보충
        private void HandleCellChanged(Slot slot, int cellIndex)
        {
            // TODO — 빈 칸 발생 감지·보충 흐름
            // if (slot.IsCellEmpty(cellIndex)) TryRefillCell(slot, cellIndex);
        }
        
        #endregion
        
        #region 접근자
        
        public Slot GetSlotByID(int slotID)
        {
            if (slotID < 0 || slotID >= _slots.Count) return null;
            return _slots[slotID];
        }
        
        #endregion
    }
}
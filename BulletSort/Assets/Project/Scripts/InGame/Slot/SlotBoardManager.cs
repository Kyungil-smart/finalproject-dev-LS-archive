using System;
using System.Collections.Generic;
using InGame.Sort.Data;
using UnityEngine;
using Logger = Core.Logger;

namespace InGame.Slot
{
    // 슬롯 9개를 통합 관리하는 보드 매니저.
    // - 슬롯 이벤트 구독 → 정렬 성공·셀 변경 응답
    // - 빈 슬롯 보충 / 보드 전체 클리어 시 재생성 + 전체 재배치 판단
    // - 외부에 정렬 성공 이벤트 통합 발행
    // "언제 채울지·재생성할지" 판단은 매니저, "무엇을 채울지"는 PieceSupplier.
    // 작성자: 이성규
    public class SlotBoardManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("자식으로 둔 슬롯 9개를 SlotID 순서로 등록")]
        [SerializeField] private List<Slot> _slots;
        
        // 기물 데이터는 PieceQuery 경유로 조회 — DB 직접 참조 없음(DataManager 데이터)
        
        [Header("Debug")]
        [SerializeField] private bool _debugMode = true;
        
        // 정렬 성공 통합 발행 — 9슬롯의 정렬 이벤트를 한 곳으로 모아 외부(포탑 소환 등)에 전달.
        // 외부는 슬롯 9개 각각이 아니라 이 이벤트 하나만 구독하면 됨.
        public event Action<int, int> OnSortSuccess;  // (slotID, pieceID)
        
        // 기물 공급기 — 대기 그룹·보충·재생성 담당.
        private PieceSupplier _supplier;
        
        private GUIStyle _debugStyle;
        
        public List<Slot> Slots => _slots;
        
        #region 유니티 라이프사이클
        
        private void Awake()
        {
            ValidateSlots();
            _supplier = new PieceSupplier();
            // DB가 가진 기물 ID 목록으로 대기 그룹 생성 — ID 체계(8001 등)는 데이터가 정함.
            _supplier.Initialize(PieceQuery.GetAllIDs());
        }
        
        private void Start()
        {
            // 구독 먼저 → 초기 배치 순서.
            // 초기 배치가 발행하는 셀 변경 이벤트는 '채우는' 행위라 보충 가드에 안 걸림.
            SubscribeSlotEvents();
            InitialPlacement();
        }
        
        private void OnDestroy()
        {
            UnsubscribeSlotEvents();
        }
        
        #endregion
        
        #region 슬롯 셋업 검증
        
        // 인스펙터 등록 상태 검증 — 리스트 비어있음·null·SlotID 인덱스 불일치 잡음.
        private void ValidateSlots()
        {
            if (_slots == null || _slots.Count == 0)
            {
                Logger.Instance.LogError("SlotBoardManager — 슬롯 리스트 비어있음");
                return;
            }
            
            // _slots[i]의 SlotID가 i와 같아야 GetSlotByID가 인덱스 = ID로 동작.
            // 인스펙터에서 순서 잘못 등록한 사고를 시작 시점에 잡기 위함.
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
        
        #region 초기 배치
        
        // 시작 시 1회 — 슬롯마다 공급기로 보충 (초기 배치 = 보충과 같은 규칙).
        private void InitialPlacement()
        {
            foreach (var slot in _slots)
                _supplier.RefillSlot(slot);
        }
        
        #endregion
        
        #region 슬롯 이벤트 응답
        
        // 슬롯 정렬 성공 응답 — 외부 통합 발행.
        // 셀 비우기는 슬롯이 자체 처리하고, 그 과정의 셀 변경 이벤트가 보충을 트리거.
        private void HandleSlotSorted(int slotID, int pieceID)
        {
            Logger.Instance.LogInfo($"보드 매니저 — 슬롯 {slotID} 정렬 성공, PieceID={pieceID}");
            OnSortSuccess?.Invoke(slotID, pieceID);
        }
        
        // 슬롯 셀 변경 응답 — 슬롯이 완전히 비었을 때만 처리.
        // 보드 전체가 비었으면 재생성 + 전체 재배치, 일부만 비었으면 그 슬롯만 보충.
        private void HandleCellChanged(Slot slot, int cellIndex)
        {
            if (!slot.IsAllEmpty()) return;  // 한 칸만 빈 건 무관 — 슬롯 전체가 비어야 처리
            
            if (IsAllSlotsEmpty())
                RegenerateBoard();   // 보드 전체 클리어 → 재생성 + 전체 재배치
            else
                _supplier.RefillSlot(slot);  // 일부 슬롯만 빔 → 그 슬롯만 보충
        }
        
        #endregion
        
        #region 보충·재생성
        
        // 보드 전체 클리어 — 대기 그룹 재생성 후 9슬롯 전부 재배치.
        // 재생성 AND 조건: 모든 슬롯 0(이미 확인됨) + 대기 그룹 0.
        // 대기 그룹에 기물이 남아있으면 재생성하지 않음(아직 한 사이클 안 끝남).
        private void RegenerateBoard()
        {
            if (!_supplier.IsEmpty) return;  // 대기 그룹 남아있으면 재생성 보류
            
            _supplier.Regenerate();
            Logger.Instance.LogInfo("보드 매니저 — 보드 전체 클리어, 대기 그룹 재생성 + 전체 재배치");
            
            foreach (var slot in _slots)
                _supplier.RefillSlot(slot);
        }
        
        // 모든 슬롯이 비어있는지 — 보드 전체 클리어 판단.
        private bool IsAllSlotsEmpty()
        {
            foreach (var slot in _slots)
                if (!slot.IsAllEmpty()) return false;
            return true;
        }
        
        #endregion
        
        #region 접근자
        
        // SlotID로 슬롯 직접 접근 — 검증에서 SlotID = 인덱스 보장됨.
        public Slot GetSlotByID(int slotID)
        {
            if (slotID < 0 || slotID >= _slots.Count) return null;
            return _slots[slotID];
        }

        public int GetConnectTowerID(int pieceID)
        {
            return PieceQuery.GetConnectTowerID(pieceID);
        }
        
        #endregion
        
        #region 디버그
        
        private void OnGUI()
        {
            if (!_debugMode || _supplier == null) return;
    
            _debugStyle ??= new GUIStyle
            {
                fontSize = 40,
                normal = { textColor = Color.white }
            };
    
            // 줄 수 = 대기 그룹 1줄 + 슬롯 수
            int lineCount = _slots.Count + 1;
            float lineHeight = 50;
            float bgHeight = lineCount * lineHeight + 20;
            float bgWidth = 1050;
    
            // 좌하단 기준 — 아래에서 위로 쌓이도록 시작 y 계산
            float startY = Screen.height - bgHeight - 5;
    
            // 반투명 배경
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(new Rect(5, startY, bgWidth, bgHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;
    
            float y = startY + 10;
    
            // 대기 그룹 먼저
            GUI.Label(new Rect(10, y, 600, 30), _supplier.GetDebugInfo(), _debugStyle);
            y += lineHeight + 5;
    
            // 슬롯별 상태
            foreach (var slot in _slots)
            {
                if (slot == null) continue;
                GUI.Label(new Rect(10, y, 600, 30), slot.GetDebugInfo(), _debugStyle);
                y += lineHeight;
            }
        }
        
        #endregion
    }
}
using System;
using System.Collections.Generic;
using Core;
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
        
        [Header("Deck (임시)")]
        [Tooltip("인게임에 투입할 덱 기물 ID. 비우면 전체(GetAllIDs) 폴백. " +
                 "정식 덱 편성 시스템 들어오면 이 자리를 덱 데이터 주입으로 교체")]
        [SerializeField] private List<int> _deckPieceIDs = new List<int>();
        
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
            ValidatePieceCount();
            
            _supplier = new PieceSupplier();
            // 덱이 지정돼 있으면 그 6개로, 비면 전체(GetAllIDs) 폴백 — 검증·디버그용.
            // 덱이 고른 ID만 대기 그룹 종류가 됨(고른 6종 × PIECE_PER_TYPE).
            // (임시) 정식은 덱 6개 필수 — 6개 미만 입장 불가 검증이 들어갈 자리. 지금은 폴백.
            // 정식 덱 편성 들어오면 _deckPieceIDs 대신 덱 데이터에서 ID 목록을 받음.
            IReadOnlyList<int> pieceIDs =
                (_deckPieceIDs != null && _deckPieceIDs.Count > 0)
                    ? _deckPieceIDs
                    : PieceQuery.GetAllIDs();
            _supplier.Initialize(pieceIDs);
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
        
        // 기물 수량 제약 검증 — 시작 시 1회. 종류·개수·대기열 규모를 바꿔도 사이클이 깨지지 않는지 확인.
        private void ValidatePieceCount()
        {
            int totalPieces = Define.PIECE_TYPE_COUNT * Define.PIECE_PER_TYPE;

            // 종류당 개수가 SORT_COUNT(3)의 배수 — 종류별 3소트가 남김없이 떨어짐.
            if (Define.PIECE_PER_TYPE % Define.SORT_COUNT != 0)
                Logger.Instance.LogWarning(
                    $"[수량 검증] Define.PIECE_PER_TYPE({Define.PIECE_PER_TYPE})를 SORT_COUNT({Define.SORT_COUNT})의 배수로 바꾸세요 " +
                    $"(현재 종류마다 {Define.PIECE_PER_TYPE % Define.SORT_COUNT}개씩 남아 3소트 안 됨). 예: {Define.PIECE_PER_TYPE / Define.SORT_COUNT * Define.SORT_COUNT} 또는 {(Define.PIECE_PER_TYPE / Define.SORT_COUNT + 1) * Define.SORT_COUNT}");

            // 전체가 보충 단위(REFILL_PER_SLOT=2)의 배수 — 대기 그룹을 2개씩 빼다 1개 남는 사고 방지.
            if (totalPieces % Define.REFILL_PER_SLOT != 0)
                Logger.Instance.LogWarning(
                    $"[수량 검증] 전체 기물 {totalPieces}개가 보충 단위 {Define.REFILL_PER_SLOT}로 안 나눠짐 — 대기 끝에 {totalPieces % Define.REFILL_PER_SLOT}개 남아 보충 안 됨. " +
                    $"Define.PIECE_TYPE_COUNT({Define.PIECE_TYPE_COUNT}) 또는 PIECE_PER_TYPE({Define.PIECE_PER_TYPE})를 조정해 전체를 짝수로 만드세요");
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
                _supplier.RefillSlot(slot, CountBoardPieces());
        }
        
        #endregion
        
        #region 슬롯 이벤트 응답
        
        // 슬롯 정렬 성공 응답 — 외부 통합 발행.
        // 셀 비우기는 슬롯이 자체 처리하고, 그 과정의 셀 변경 이벤트가 보충을 트리거.
        private void HandleSlotSorted(int slotID, int pieceID)
        {
            Logger.Instance.LogInfo($"보드 매니저 — 슬롯 {slotID} 정렬 성공, PieceID={pieceID}");
            OnSortSuccess?.Invoke(slotID, pieceID);
            
            // ── 임시: 슬롯 비주얼 교체를 정렬 성공에 직결 ──
            // 정식 흐름은 포탑 시스템이 가동 포탑(ActiveTurret) 확정 시 SetTowerType 호출.
            // 지금은 ActiveTurret 신호가 없어, 정렬 성공 즉시 연결 타워 이미지로 바꿔 검증만.
            // towerType은 임시로 ConnectTowerID 직접 사용 — 현재 1·2·3이 TowerType(1~6) 범위와 겹쳐 동작.
            // 타워 DB 정식화 시 ConnectTowerID는 실제 타워 ID(7001 등)가 되므로,
            //   GetConnectTowerID → TowerData.TowerType 변환 단계를 끼워야 함.
            int towerType = GetConnectTowerID(pieceID);
            GetSlotByID(slotID)?.Visual?.SetTowerType(towerType);
        }
        
        // 슬롯 셀 변경 응답 — 슬롯이 완전히 비었을 때만 처리.
        // 보드 전체가 비었으면 재생성 + 전체 재배치, 일부만 비었으면 그 슬롯만 보충.
        private void HandleCellChanged(Slot slot, int cellIndex)
        {
            if (!slot.IsAllEmpty()) return;  // 한 칸만 빈 건 무관 — 슬롯 전체가 비어야 처리
            
            if (IsAllSlotsEmpty())
                RegenerateBoard();   // 보드 전체 클리어 → 재생성 + 전체 재배치
            else
                _supplier.RefillSlot(slot, CountBoardPieces());  // 일부 슬롯만 빔 → 그 슬롯만 보충
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
                _supplier.RefillSlot(slot, CountBoardPieces());
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
        
        // 보드 전체 기물 카운트 집계 — 9슬롯 순회, PieceSelector 우선순위 판정용.
        private Dictionary<int, int> CountBoardPieces()
        {
            var counts = new Dictionary<int, int>();
            foreach (var slot in _slots)
                slot?.AccumulatePieceCounts(counts);
            return counts;
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
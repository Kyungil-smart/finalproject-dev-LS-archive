using System.Collections.Generic;
using Core;
using InGame.Sort.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lobby.Deck
{
    // 덱 편성 — 보유 목록(18그룹 현재 레벨, 미보유 포함) 생성, 카드 탭으로 6칸 편성/해제.
    // 시작: 편성 6개 ID를 DeckHolder에 저장 → 인게임 씬 진입(SlotBoardManager가 읽음).
    //   ※ 시작 진입점은 스테이지 선택(StageSelectController)으로 일원화 — 여기 시작 버튼 없음.
    //     스테이지 선택이 SetStageID 후 OnTapStart()를 호출.
    // 미보유 카드는 표시만 되고 편성 불가(해금은 강화창).
    // 정렬은 카드 재생성 없이 SetActive 토글 — 편성 상태 유지, GridLayout이 재배치.
    // 작성자: 이성규
    public class DeckBuilder : MonoBehaviour
    {
        [Header("편성 슬롯 (6칸 고정)")]
        [SerializeField] private DeckSlot[] _slots;

        [Header("보유 목록")]
        [SerializeField] private Transform _ownedContent;    // ScrollView/Viewport/Content
        [SerializeField] private DeckCard _ownedCardPrefab;  // 동적 생성용 프리팹

        [Header("버튼")]
        [Tooltip("공격 유형 보기 — i 버튼")]
        [SerializeField] private Button _attackTypeButton;

        [Tooltip("정렬(필터) 버튼")]
        [SerializeField] private Button _sortButton;

        [Header("정보 텍스트")]
        [Tooltip("편성 수 — (없음)/(n명)/(완료). 앞 문구는 고정 텍스트 오브젝트")]
        [SerializeField] private TMP_Text _equippedCountText;

        [Tooltip("보유 수 — (n명). 미보유 카드는 제외")]
        [SerializeField] private TMP_Text _ownedCountText;

        // 보유 카드 인스턴스 — PieceID로 찾아 편성 상태(SetInDeck) 갱신
        private readonly List<DeckCard> _ownedCards = new List<DeckCard>();

        // 현재 보유 목록 필터 — 정렬 팝업이 갱신. 기본 전체.
        private readonly SortFilter _filter = new SortFilter();

        // ---- 초기화 ----

        private void Start()
        {
            DeckHolder.Clear();

            InitSlots();
            BuildOwnedList();
            RefreshCounts();

            if (_attackTypeButton != null)
                _attackTypeButton.onClick.AddListener(() => PopupManager.Instance.ShowAttackType());

            if (_sortButton != null)
                _sortButton.onClick.AddListener(OnTapSort);
        }

        // 편성 슬롯 6칸 초기화 — 빈 칸 + 탭(해제) 콜백 등록
        private void InitSlots()
        {
            foreach (var slot in _slots)
                slot.Init(OnTapEquippedSlot);
        }

        // 보유 목록 생성 — 18개 그룹(이름·성급)의 현재 레벨 카드. 미보유도 표시(오버레이).
        //   강화하면 그룹의 현재 레벨 ID가 바뀌므로 목록을 다시 그려야 반영됨.
        private void BuildOwnedList()
        {
            var ids = PieceQuery.GetInventoryIDs();
            foreach (var id in ids)
            {
                var card = Instantiate(_ownedCardPrefab, _ownedContent);
                card.Setup(id, OnTapOwnedCard, OnLongPressOwnedCard);
                _ownedCards.Add(card);
            }
        }

        // ---- 입력 핸들러 ----

        // 보유 카드 탭 → 편성 안 됐으면 빈 슬롯에 편성, 이미 편성됐으면 해제(재터치).
        //   미보유 카드도 오버레이 위 버튼으로 여기 오지만 IsOwned로 차단.
        private void OnTapOwnedCard(DeckCard card)
        {
            if (!card.IsOwned) return;   // 미보유 — 편성 불가(해금은 강화창)

            if (IsEquipped(card.PieceID))
            {
                UnequipByPieceID(card.PieceID);
                return;
            }

            var empty = FindEmptySlot();
            if (empty == null) return;   // 6칸 다 참 → 무시(정식은 안내)

            empty.SetPiece(card.PieceID);
            card.SetInDeck(true);        // 보유 카드 잠금(편성 중 오버레이 ON)
            RefreshCounts();
        }
        
        // 카드 길게 누르기 → 상세보기 팝업. 미보유도 열림(편성 버튼만 비활성).
        private void OnLongPressOwnedCard(DeckCard card)
        {
            PopupManager.Instance.ShowPieceDetail(card.PieceID, card.IsOwned, EquipByPieceID);
        }

        // 상세보기의 "편성하기" — 이미 편성 중이거나 6칸 차면 무시.
        //   카드 탭 편성(OnTapOwnedCard)과 같은 결과.
        private void EquipByPieceID(int pieceID)
        {
            if (IsEquipped(pieceID)) return;

            var empty = FindEmptySlot();
            if (empty == null) return;

            empty.SetPiece(pieceID);
            SetOwnedInDeck(pieceID, true);
            RefreshCounts();
        }

        // 편성 슬롯 탭 → 해제 + 해당 보유 카드 잠금 해제
        private void OnTapEquippedSlot(DeckSlot slot)
        {
            int pieceID = slot.PieceID;
            slot.SetEmpty();
            SetOwnedInDeck(pieceID, false);
            RefreshCounts();
        }

        // PieceID로 편성 슬롯을 역추적해 해제 — 보유 카드 재터치 해제용.
        //   슬롯 탭 해제(OnTapEquippedSlot)와 같은 결과(슬롯 비움 + 보유 카드 잠금 해제).
        private void UnequipByPieceID(int pieceID)
        {
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.PieceID == pieceID)
                {
                    slot.SetEmpty();
                    SetOwnedInDeck(pieceID, false);
                    RefreshCounts();
                    return;
                }
            }
        }

        // 정렬 버튼 → 팝업. 확인하면 필터 갱신 후 목록 표시 상태만 갱신.
        private void OnTapSort()
        {
            PopupManager.Instance.ShowSort(_filter, ApplyFilter);
        }

        // 필터 적용 — 카드 표시/숨김만 토글(재생성 안 함, 편성 상태 유지).
        private void ApplyFilter(SortFilter filter)
        {
            _filter.CopyFrom(filter);

            foreach (var card in _ownedCards)
            {
                int type = PieceQuery.GetConnectTowerType(card.PieceID);
                card.gameObject.SetActive(_filter.Pass(card.IsOwned, type));
            }
        }

        // ---- 시작 ----

        // 6칸 다 편성됐으면 인게임 진입, 미달이면 경고(입장 불가).
        //   StageID는 스테이지 선택에서 이미 세팅 — 여기선 덱만 챙김.
        public void OnTapStart()
        {
            if (!IsDeckFull())
            {
                PopupManager.Instance.ShowAlert("캐릭터 편성이 부족합니다.\n6개의 캐릭터를 편성해 주세요.");
                return;
            }

            DeckHolder.Set(CollectDeckIDs());
            SceneManager.LoadScene(Define.SCENE_INGAME);
        }

        // ---- 조회 헬퍼 ----

        private bool IsDeckFull()
        {
            foreach (var slot in _slots)
                if (slot.IsEmpty) return false;
            return true;
        }

        private bool IsEquipped(int pieceID)
        {
            foreach (var slot in _slots)
                if (!slot.IsEmpty && slot.PieceID == pieceID) return true;
            return false;
        }

        private DeckSlot FindEmptySlot()
        {
            foreach (var slot in _slots)
                if (slot.IsEmpty) return slot;
            return null;
        }

        // 편성된 6칸의 PieceID 수집 — OnTapStart에서 6칸 보장 후 호출(폴백 불필요).
        private List<int> CollectDeckIDs()
        {
            var deck = new List<int>(_slots.Length);
            foreach (var slot in _slots)
                if (!slot.IsEmpty) deck.Add(slot.PieceID);
            return deck;
        }

        // 보유 목록에서 해당 PieceID 카드를 찾아 편성 상태 갱신
        private void SetOwnedInDeck(int pieceID, bool inDeck)
        {
            foreach (var card in _ownedCards)
                if (card.PieceID == pieceID)
                {
                    card.SetInDeck(inDeck);
                    return;
                }
        }

        // ---- 표시 갱신 ----

        // 편성·보유 수 표시. 덱 상태가 바뀔 때마다 호출.
        //   편성: 0=없음 / 1~5=n명 / 6=완료(6칸 고정). 보유: 목록 중 IsOwned인 카드 수(필터 무관).
        //   ※ 괄호만 코드가 채움 — 앞 문구는 고정 텍스트 오브젝트(로컬라이즈 대비).
        private void RefreshCounts()
        {
            if (_equippedCountText != null)
            {
                int equipped = 0;
                foreach (var slot in _slots)
                    if (!slot.IsEmpty) equipped++;

                string state = equipped == 0 ? "없음"
                             : equipped >= _slots.Length ? "완료"
                             : $"{equipped}명";

                _equippedCountText.text = $"({state})";
            }

            if (_ownedCountText != null)
            {
                int owned = 0;
                foreach (var card in _ownedCards)
                    if (card.IsOwned) owned++;

                _ownedCountText.text = $"({owned}명)";
            }
        }
    }
}
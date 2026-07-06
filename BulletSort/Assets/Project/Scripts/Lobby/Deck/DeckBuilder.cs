using Core;
using InGame.Sort.Data;
using Lobby.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lobby.Deck
{
    // 덱 편성 관리 — 보유 목록(성급/캐릭터별 대표 카드) 동적 생성 + 편성 슬롯 6칸 관리 + 탭 편성/해제 + 시작.
    // 빠른 구현: 보유 = PieceQuery.GetRepresentativeIDs(), 정식 보유 풀은 김경민 데이터 후.
    // 탭 방식: 보유 카드 탭 → 빈 슬롯 편성 + 그 카드 "편성 중" 잠금 / 편성 카드 탭 → 해제 + 잠금 해제.
    // 시작: 편성 6개 ID를 DeckHolder에 저장 → 인게임 씬 진입(SlotBoardManager가 읽음).
    // 작성자: 이성규
    public class DeckBuilder : MonoBehaviour
    {
        [Header("편성 슬롯 (6칸 고정)")]
        [SerializeField] private DeckSlot[] _slots;        // DeckSlot 6개

        [Header("보유 목록")]
        [SerializeField] private Transform _ownedContent;  // ScrollView/Viewport/Content
        [SerializeField] private DeckCard _ownedCardPrefab; // 동적 생성용 프리팹

        [Header("시작")]
        [SerializeField] private Button _startButton;       // 시작 버튼(없으면 외부에서 OnTapStart 호출)

        // 보유 카드 인스턴스 — PieceID로 찾아 편성 상태(SetInDeck) 갱신
        private readonly List<DeckCard> _ownedCards = new List<DeckCard>();

        private void Start()
        {
            DeckHolder.Clear();

            InitSlots();
            BuildOwnedList();

            if (_startButton != null)
                _startButton.onClick.AddListener(OnTapStart);
        }

        // 편성 슬롯 6칸 초기화 — 빈 칸 + 탭(해제) 콜백 등록
        private void InitSlots()
        {
            foreach (var slot in _slots)
                slot.Init(OnTapEquippedSlot);
        }

        // 보유 목록 생성 — 카드 단위 대표(성급/캐릭터별 1장). 정식은 유저 보유 풀.
        private void BuildOwnedList()
        {
            var ids = PieceQuery.GetRepresentativeIDs();
            foreach (var id in ids)
            {
                var card = Instantiate(_ownedCardPrefab, _ownedContent);
                card.Setup(id, OnTapOwnedCard);
                _ownedCards.Add(card);
            }
        }

        // 보유 카드 탭 → 편성 안 됐으면 빈 슬롯에 편성, 이미 편성됐으면 해제(재터치).
        //   편성 중 카드는 오버레이 위 _inDeckButton이 클릭을 받아 여기로 옴(재터치 해제 경로).
        private void OnTapOwnedCard(DeckCard card)
        {
            // 이미 편성 중이면 재터치 → 해제. 그 카드가 든 슬롯을 찾아 비우고 잠금 해제.
            if (IsEquipped(card.PieceID))
            {
                UnequipByPieceID(card.PieceID);
                return;
            }

            var empty = FindEmptySlot();
            if (empty == null) return;   // 6칸 다 참 → 무시(정식은 안내)

            empty.SetPiece(card.PieceID);
            card.SetInDeck(true);        // 보유 카드 잠금(편성 중 오버레이 ON)
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
                    SetOwnedInDeck(pieceID, false);  // 보유 카드 다시 누를 수 있게
                    return;
                }
            }
        }

        // 편성 슬롯 탭 → 해제 + 해당 보유 카드 잠금 해제
        private void OnTapEquippedSlot(DeckSlot slot)
        {
            int pieceID = slot.PieceID;
            slot.SetEmpty();
            SetOwnedInDeck(pieceID, false);  // 보유 카드 다시 누를 수 있게
        }

        // 시작 — 6칸 다 편성됐으면 인게임 진입, 미달이면 경고(입장 불가).
        public void OnTapStart()
        {
            // 미편성(6칸 미만) — 경고 팝업 띄우고 입장 막음(기획: 6칸 필수).
            if (!IsDeckFull())
            {
                PopupManager.Instance.ShowAlert("캐릭터 편성이 부족합니다.\n6개의 캐릭터를 편성해 주세요.");
                return;
            }

            // 임시로 1001 Stage를 넣어둠.
            StageManager.Instance.SetStageID(1001);

            var deck = CollectDeckIDs();
            DeckHolder.Set(deck);
            SceneManager.LoadScene(Define.SCENE_INGAME);
        }

        // 편성 슬롯 6칸이 모두 찼는지 — 하나라도 비면 false.
        private bool IsDeckFull()
        {
            foreach (var slot in _slots)
                if (slot.IsEmpty) return false;
            return true;
        }

        // 편성된 6칸의 PieceID 수집 — OnTapStart에서 6칸 보장 후 호출(폴백 불필요).
        private List<int> CollectDeckIDs()
        {
            var deck = new List<int>();
            foreach (var slot in _slots)
                if (!slot.IsEmpty) deck.Add(slot.PieceID);
            return deck;
        }

        // PieceID로 보유 카드 찾아 편성 상태 갱신
        private void SetOwnedInDeck(int pieceID, bool inDeck)
        {
            foreach (var card in _ownedCards)
                if (card.PieceID == pieceID) { card.SetInDeck(inDeck); return; }
        }

        private DeckSlot FindEmptySlot()
        {
            foreach (var slot in _slots)
                if (slot.IsEmpty) return slot;
            return null;
        }

        private bool IsEquipped(int pieceID)
        {
            foreach (var slot in _slots)
                if (!slot.IsEmpty && slot.PieceID == pieceID) return true;
            return false;
        }
    }
}
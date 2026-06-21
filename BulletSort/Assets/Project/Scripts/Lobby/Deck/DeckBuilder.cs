using System.Collections.Generic;
using InGame.Sort.Data;
using UnityEngine;

namespace Lobby.Deck
{
    // 덱 편성 관리 — 보유 목록(SO 18개) 동적 생성 + 편성 슬롯 6칸 관리 + 탭 편성/해제.
    // 빠른 구현: 보유 = PieceQuery.GetAllIDs()(임시 18개), 정식 보유 풀은 김경민 데이터 후.
    // 탭 방식: 보유 카드 탭 → 빈 슬롯 편성 + 그 카드 "편성 중" 잠금 / 편성 카드 탭 → 해제 + 잠금 해제.
    // 작성자: 이성규
    public class DeckBuilder : MonoBehaviour
    {
        [Header("편성 슬롯 (6칸 고정)")]
        [SerializeField] private DeckSlot[] _slots;        // DeckSlot 6개

        [Header("보유 목록")]
        [SerializeField] private Transform _ownedContent;  // ScrollView/Viewport/Content
        [SerializeField] private DeckCard _ownedCardPrefab; // 동적 생성용 프리팹

        // 보유 카드 인스턴스 — PieceID로 찾아 편성 상태(SetInDeck) 갱신
        private readonly List<DeckCard> _ownedCards = new List<DeckCard>();

        private void Start()
        {
            InitSlots();
            BuildOwnedList();
        }

        // 편성 슬롯 6칸 초기화 — 빈 칸 + 탭(해제) 콜백 등록
        private void InitSlots()
        {
            foreach (var slot in _slots)
                slot.Init(OnTapEquippedSlot);
        }

        // 보유 목록 생성 — 임시로 전체 ID(18개)를 카드로. 정식은 유저 보유 풀.
        private void BuildOwnedList()
        {
            var ids = PieceQuery.GetAllIDs();
            foreach (var id in ids)
            {
                var card = Instantiate(_ownedCardPrefab, _ownedContent);
                card.Setup(id, OnTapOwnedCard);
                _ownedCards.Add(card);
            }
        }

        // 보유 카드 탭 → 빈 슬롯에 편성 + 그 카드 "편성 중" 잠금
        private void OnTapOwnedCard(DeckCard card)
        {
            // 편성 중 카드는 오버레이가 클릭을 흡수해 여기 안 옴(이중 가드).
            if (IsEquipped(card.PieceID)) return;

            var empty = FindEmptySlot();
            if (empty == null) return;   // 6칸 다 참 → 무시(정식은 안내)

            empty.SetPiece(card.PieceID);
            card.SetInDeck(true);        // 보유 카드 잠금(편성 중 오버레이 ON)
        }

        // 편성 슬롯 탭 → 해제 + 해당 보유 카드 잠금 해제
        private void OnTapEquippedSlot(DeckSlot slot)
        {
            int pieceID = slot.PieceID;
            slot.SetEmpty();
            SetOwnedInDeck(pieceID, false);  // 보유 카드 다시 누를 수 있게
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
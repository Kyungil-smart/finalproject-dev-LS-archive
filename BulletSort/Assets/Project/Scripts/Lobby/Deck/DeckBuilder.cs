using Core;
using InGame.Sort.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lobby.Deck
{
    // 덱 편성 관리 — 보유 목록(SO 18개) 동적 생성 + 편성 슬롯 6칸 관리 + 탭 편성/해제 + 시작.
    // 빠른 구현: 보유 = PieceQuery.GetAllIDs()(임시 18개), 정식 보유 풀은 김경민 데이터 후.
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

        // 시작 — 편성 덱을 DeckHolder에 넘기고 인게임 진입.
        // 6칸이 다 안 찼으면 (임시) 기초 폴백으로 빈 칸을 채워 진입.
        //   TODO: 폴백 대신 경고창("덱을 모두 채워주세요")으로 교체 — 6칸 미만 입장 불가.
        public void OnTapStart()
        {
            // 임시로 1001 Stage를 넣어둠.
            StageManager.Instance.SetStageID(1001);
            var deck = CollectDeckIDs();
            DeckHolder.Set(deck);
            SceneManager.LoadScene(Define.SCENE_INGAME);
        }

        // 편성된 ID 수집. 빈 칸은 (임시) 폴백 ID로 채움.
        private List<int> CollectDeckIDs()
        {
            var deck = new List<int>();
            foreach (var slot in _slots)
                if (!slot.IsEmpty) deck.Add(slot.PieceID);

            // (임시) 6칸 미만이면 기초 폴백으로 채움 — 전체 풀에서 미편성 ID를 끌어옴.
            //   정식은 여기서 막고 경고창. 지금은 진입은 되게.
            if (deck.Count < _slots.Length)
                FillFallback(deck);

            return deck;
        }

        // 부족분을 전체 풀의 미편성 ID로 채움 (임시 폴백)
        private void FillFallback(List<int> deck)
        {
            var all = PieceQuery.GetAllIDs();
            foreach (var id in all)
            {
                if (deck.Count >= _slots.Length) break;
                if (!deck.Contains(id)) deck.Add(id);
            }
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
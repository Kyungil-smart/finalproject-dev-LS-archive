using System.Collections;
using System.Collections.Generic;
using Core;
using InGame.Sort.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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
    // 현지화 — 편성/보유 수는 문구 전체가 테이블 엔트리(접두어 포함). Count류는 {0}에 인원을 넘김.
    //   코드가 조립하는 문구이므로 해당 TMP에 LocalizeStringEvent를 붙이면 안 됨(비동기 갱신이 덮어씀).
    //   테이블 로드 전 GetLocalizedString은 빈 값 → 로드 완료를 기다린 뒤 첫 표시.
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
        [Tooltip("편성 수 — 문구 전체를 코드가 채움(접두어 포함). LocalizeStringEvent 부착 금지")]
        [SerializeField] private TMP_Text _equippedCountText;

        [Tooltip("보유 수 — 문구 전체를 코드가 채움(접두어 포함). LocalizeStringEvent 부착 금지")]
        [SerializeField] private TMP_Text _ownedCountText;

        [Header("현지화 문구")]
        [Tooltip("Deck_Selected_BulletGirls_None — 편성 0")]
        [SerializeField] private LocalizedString _equippedNone;

        [Tooltip("Deck_Selected_BulletGirls_Count — 편성 1~5, {0}에 인원")]
        [SerializeField] private LocalizedString _equippedCount;

        [Tooltip("Deck_Selected_BulletGirls_Full — 편성 6(완료)")]
        [SerializeField] private LocalizedString _equippedFull;

        [Tooltip("Deck_Owned_BulletGirls_Count — 보유 수, {0}에 인원")]
        [SerializeField] private LocalizedString _ownedCount;

        // 로컬라이즈 테이블 이름 — 프로젝트 공용 단일 테이블(StageSelectController와 동일).
        private const string LOCALIZATION_TABLE = "LocalizationTable";

        // 보유 카드 인스턴스 — PieceID로 찾아 편성 상태(SetInDeck) 갱신
        private readonly List<DeckCard> _ownedCards = new List<DeckCard>();

        // 현재 보유 목록 필터 — 정렬 팝업이 갱신. 기본 전체.
        private readonly SortFilter _filter = new SortFilter();

        // 현지화 테이블 로드 완료 여부 — 완료 전엔 문구 갱신을 건너뜀(빈 문자열 대입 방지).
        //   언어 전환 시 테이블이 다시 로드되므로 false로 되돌린 뒤 대기 코루틴이 다시 세운다.
        private bool _localizationReady;

        // ---- 생명주기 ----

        // 최초 1회 초기화 여부 — 탭 전환으로 OnEnable이 반복되므로 플래그로 가드.
        //   Start를 안 쓰는 이유: 코루틴이 GameObject 비활성 시 중단되고 재개되지 않음.
        //   (로비 진입 시 LobbyTabBar가 덱 윈도우를 끄면 Start 코루틴이 죽는다)
        private bool _initialized;

        private void OnEnable()
        {
            PieceInventory.OnChanged += RefreshOwnedList;
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;

            if (!_initialized)
            {
                _initialized = true;
                InitOnce();
            }

            // 현지화 테이블 로드를 기다린 뒤 갱신 — 탭이 켜져 있는 동안만 도는 코루틴.
            StartCoroutine(RefreshWhenLocalizationReady());
        }

        private void OnDisable()
        {
            PieceInventory.OnChanged -= RefreshOwnedList;
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        // 최초 1회 — 슬롯·보유 목록·버튼 리스너.
        private void InitOnce()
        {
            DeckHolder.Clear();

            InitSlots();
            BuildOwnedList();

            if (_attackTypeButton != null)
                _attackTypeButton.onClick.AddListener(() => PopupManager.Instance.ShowAttackType());

            if (_sortButton != null)
                _sortButton.onClick.AddListener(OnTapSort);
        }

        // 언어 전환 — SelectedLocale 대입 직후 동기로 발행되므로 테이블이 아직 로딩 중일 수 있음.
        //   즉시 갱신하면 GetLocalizedString이 빈 값을 반환 → 로드를 기다린 뒤 갱신.
        //   꺼진 탭은 코루틴을 못 돌리므로 스킵 — 다음 OnEnable에서 갱신된다.
        private void OnLanguageChanged()
        {
            if (!isActiveAndEnabled) return;

            _localizationReady = false;
            StartCoroutine(RefreshWhenLocalizationReady());
        }

        // 테이블 로드 전 GetLocalizedString은 빈 값 — 완료 후 갱신.
        //   InitializationOperation은 최초 1회만 유효 → 전환 시엔 테이블 핸들도 함께 대기.
        private IEnumerator RefreshWhenLocalizationReady()
        {
            yield return LocalizationSettings.InitializationOperation;

            var table = LocalizationSettings.StringDatabase.GetTableAsync(LOCALIZATION_TABLE);
            yield return table;

            _localizationReady = true;
            RefreshOwnedList();
        }

        // ---- 초기화 ----

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

        // 인벤토리 변경(해금·강화)·언어 변경 → 카드·슬롯 갱신. 재생성 안 함(스크롤 위치·편성 유지).
        //   순서 중요 — 슬롯 ID를 먼저 현재 레벨로 맞춰야 IsEquipped 조회가 새 ID와 일치.
        private void RefreshOwnedList()
        {
            RemapSlots();

            var ids = PieceQuery.GetInventoryIDs();
            for (int i = 0; i < _ownedCards.Count && i < ids.Count; i++)
            {
                _ownedCards[i].Setup(ids[i], OnTapOwnedCard, OnLongPressOwnedCard);
                _ownedCards[i].SetInDeck(IsEquipped(ids[i]));   // Setup이 false로 초기화하므로 다시 씌움
            }

            ApplyFilter(_filter);   // 미보유 → 보유로 바뀌면 필터 통과 여부도 달라짐
            RefreshCounts();
        }

        // 편성 슬롯의 PieceID를 현재 레벨로 재매핑 — 강화로 그룹의 대표 ID가 바뀌었을 수 있음.
        //   같은 (이름·성급)이면 편성은 유지하고 ID만 최신으로. 슬롯 비주얼도 새 레벨로 갱신됨.
        private void RemapSlots()
        {
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty) continue;

                var data = PieceQuery.Get(slot.PieceID);
                if (data == null) continue;

                int lv = PieceInventory.GetLevel(data.PieceName, data.PieceGrade);
                int id = PieceQuery.GetIDByGroup(data.PieceName, data.PieceGrade, lv);

                if (id != 0 && id != slot.PieceID) slot.SetPiece(id);
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

        // 편성·보유 수 표시. 덱 상태·언어가 바뀔 때마다 호출.
        //   문구 전체가 테이블 엔트리(접두어 포함). Count류는 {0}에 인원을 넘김.
        //   편성: 0=None / 1~5=Count / 6=Full. 보유: 목록 중 IsOwned인 카드 수(필터 무관).
        //   ※ 테이블 로드 전 호출은 무시 — 빈 문자열이 대입되면 프리셋 문구까지 지워짐.
        private void RefreshCounts()
        {
            if (!_localizationReady) return;

            if (_equippedCountText != null)
            {
                int equipped = 0;
                foreach (var slot in _slots)
                    if (!slot.IsEmpty) equipped++;

                if (equipped == 0)
                    _equippedCountText.text = _equippedNone.GetLocalizedString();
                else if (equipped >= _slots.Length)
                    _equippedCountText.text = _equippedFull.GetLocalizedString();
                else
                    _equippedCountText.text = _equippedCount.GetLocalizedString(equipped);
            }

            if (_ownedCountText != null)
            {
                int owned = 0;
                foreach (var card in _ownedCards)
                    if (card.IsOwned) owned++;

                _ownedCountText.text = _ownedCount.GetLocalizedString(owned);
            }
        }
    }
}
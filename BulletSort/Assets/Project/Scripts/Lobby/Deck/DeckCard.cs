using System;
using InGame.Sort.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Deck
{
    // 덱 카드 — 편성 슬롯·보유 목록 공용 프리팹. PieceID로 자기 비주얼을 채우고(Setup),
    // 탭하면 등록된 콜백을 호출(편성/해제는 호출부가 결정).
    // 상태 오버레이 — 미보유(Status_NotOwned)·편성중(Status_InDeck) 공통 딤(Status_Overlay).
    //   그 딤 Image의 Raycast Target ON이라 켜지면 카드를 덮어 아래 버튼 클릭을 흡수.
    //   단 딤 위 _inDeckButton이 그 클릭을 받아 같은 _onTap을 호출 — 보유 목록에선 편성 중 카드
    //   재터치로 해제 가능(호출부가 분기). 미보유 카드도 여기로 오지만 호출부가 IsOwned로 차단.
    // 보유 여부는 카드가 PieceInventory에서 직접 조회 — 호출부는 신경 안 씀.
    // 작성자: 이성규
    public class DeckCard : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _portrait;          // 초상화 (RectMask2D 자식)
        [SerializeField] private Image _frame;             // 카드 프레임 (타입별)
        [SerializeField] private Image _background;        // 카드 배경 (타입별)
        [SerializeField] private Image _typeIcon;          // 공격 유형 아이콘 (TypeInfo/Type_Image)
        [SerializeField] private TMP_Text _lvText;
        [SerializeField] private GameObject[] _stars;      // Star_0~2, Grade만큼 ON

        [Header("Status")]
        [SerializeField] private GameObject _statusOverlay;  // 미보유/편성중 공통 딤 (Raycast로 클릭 차단)
        [SerializeField] private GameObject _statusInDeck;   // "편성 중"
        [SerializeField] private GameObject _statusNotOwned; // "미보유"

        [SerializeField] private Button _button;

        [Tooltip("오버레이 위 버튼 — 편성 중 카드 재터치 해제용. 카드 버튼과 같은 _onTap으로.")]
        [SerializeField] private Button _inDeckButton;
        
        [Tooltip("길게 누르기 — 상세보기 진입. 같은 오브젝트에 부착")]
        [SerializeField] private LongPressHandler _longPress;
        
        [Header("강화창 전용")]
        [Tooltip("선택 강조 테두리")]
        [SerializeField] private GameObject _selectHighlight;

        [Tooltip("상단 라벨 — Select / Level Up / Buy")]
        [SerializeField] private GameObject _statusTop;
        [SerializeField] private Image _statusImage;

        [Tooltip("하단 비용 — 보유/필요")]
        [SerializeField] private GameObject _costArea;
        [SerializeField] private Image _costIcon;
        [SerializeField] private TMP_Text _costText;

        public int PieceID { get; private set; }

        // 보유 여부 — 미보유면 편성 불가(호출부가 판단). Setup에서 PieceInventory 조회로 확정.
        public bool IsOwned { get; private set; }

        // 탭 콜백 — 호출부(DeckBuilder)가 자기 처리를 등록. 카드는 누가 눌렸는지만 전달.
        private Action<DeckCard> _onTap;
        private Action<DeckCard> _onLongPress;

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);

            // 오버레이 위 버튼 — 딤이 켜졌을 때(미보유·편성중) 클릭을 대신 받음.
            if (_inDeckButton != null)
                _inDeckButton.onClick.AddListener(OnClick);

            if (_longPress != null)
                _longPress.OnLongPress += () => _onLongPress?.Invoke(this);
        }

        // 탭 — 롱프레스로 소비된 누름이면 무시(상세보기가 떴으므로 편성하지 않음).
        private void OnClick()
        {
            if (_longPress != null && _longPress.Consumed) return;
            _onTap?.Invoke(this);
        }

        // PieceID로 비주얼 채움. 호출부는 이 한 줄 + 콜백만.
        //   onLongPress는 보유 목록에서만 씀(편성 슬롯은 null).
        public void Setup(int pieceID, Action<DeckCard> onTap, Action<DeckCard> onLongPress = null)
        {
            PieceID = pieceID;
            _onTap = onTap;
            _onLongPress = onLongPress;

            var data = PieceQuery.Get(pieceID);
            if (data == null) return;

            if (_portrait != null) _portrait.sprite = PieceQuery.GetPortrait(pieceID);
            if (_frame != null) _frame.sprite = PieceQuery.GetCardFrame(pieceID);
            if (_background != null) _background.sprite = PieceQuery.GetCardBackground(pieceID);
            if (_typeIcon != null) _typeIcon.sprite = PieceQuery.GetTypeIcon(pieceID);
            if (_lvText != null) _lvText.text = $"Lv {data.PieceLv}";
            SetStars(data.PieceGrade);

            IsOwned = PieceInventory.IsOwned(data.PieceName, data.PieceGrade);
            RefreshStatus(inDeck: false);
        }

        // 편성 중 표시 토글 — 호출부가 편성/해제 시 호출.
        public void SetInDeck(bool inDeck) => RefreshStatus(inDeck);

        // 상태 오버레이 갱신 — 미보유가 편성 중보다 우선(미보유는 애초에 편성 불가).
        //   딤은 둘 중 하나라도 참이면 ON → 카드가 어두워지고 클릭이 막힘.
        private void RefreshStatus(bool inDeck)
        {
            bool notOwned = !IsOwned;

            if (_statusOverlay != null) _statusOverlay.SetActive(notOwned || inDeck);
            if (_statusNotOwned != null) _statusNotOwned.SetActive(notOwned);
            if (_statusInDeck != null) _statusInDeck.SetActive(!notOwned && inDeck);
        }

        // 성급만큼 별 켜기 (1~3 고정, 별 3개 중 Grade개 ON)
        private void SetStars(int grade)
        {
            if (_stars == null) return;
            for (int i = 0; i < _stars.Length; i++)
                if (_stars[i] != null) _stars[i].SetActive(i < grade);
        }
        
        // ---- 강화창 전용 표시 ----
        //   덱 편성에선 아무것도 안 부름 → 프리팹 기본값(꺼짐) 유지.

        // 선택 강조 토글.
        public void SetSelected(bool on)
        {
            if (_selectHighlight != null) _selectHighlight.SetActive(on);
        }

        // 상단 라벨 — null이면 끔.
        public void SetStatusLabel(Sprite label)
        {
            bool on = label != null;
            if (_statusTop != null) _statusTop.SetActive(on);
            if (on && _statusImage != null) _statusImage.sprite = label;
        }

        // 하단 비용 — have/need.
        public void SetCost(Sprite icon, int have, int need)
        {
            if (_costArea != null) _costArea.SetActive(true);
            if (_costIcon != null && icon != null) _costIcon.sprite = icon;
            if (_costText != null) _costText.text = $"{have}/{need}";
        }

        public void HideCost()
        {
            if (_costArea != null) _costArea.SetActive(false);
        }
    }
}
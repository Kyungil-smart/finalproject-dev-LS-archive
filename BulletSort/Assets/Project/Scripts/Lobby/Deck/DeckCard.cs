using System;
using InGame.Sort.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Deck
{
    // 덱 카드 — 편성 슬롯·보유 목록 공용 프리팹. PieceID로 자기 비주얼을 채우고(Setup),
    // 탭하면 등록된 콜백을 호출(편성/해제는 호출부가 결정).
    // 편성 중 표시는 Status_Overlay 토글 — 그 오버레이 Image의 Raycast Target ON이라
    // 켜지면 카드를 덮어 아래 버튼 클릭을 흡수. 단 오버레이 위 _inDeckButton이 그 클릭을
    //   받아 같은 _onTap을 호출 — 보유 목록에선 편성 중 카드 재터치로 해제 가능(호출부가 분기).
    // 작성자: 이성규
    public class DeckCard : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _portrait;          // 초상화 (RectMask2D 자식)
        [SerializeField] private Image _frame;             // 카드 프레임 (PieceType별)
        [SerializeField] private Image _background;        // 카드 배경 (PieceType별)
        [SerializeField] private Image _typeIcon;          // 공격 유형 아이콘 (TypeInfo/Type_Image)
        [SerializeField] private TMP_Text _lvText;
        [SerializeField] private GameObject[] _stars;      // Star_0~2, Grade만큼 ON
        [SerializeField] private GameObject _statusOverlay; // 편성중/미보유 묶음 (Raycast로 클릭 차단)
        [SerializeField] private GameObject _statusInDeck;  // "편성 중"
        [SerializeField] private GameObject _statusNotOwned;// "미보유" (후순위)
        [SerializeField] private Button _button;

        [Tooltip("편성 중 오버레이 버튼 — '편성 중'(Status_InDeck)을 재터치하면 해제. 카드 버튼과 같은 _onTap으로.")]
        [SerializeField] private Button _inDeckButton;

        public int PieceID { get; private set; }

        // 탭 콜백 — 호출부(DeckBuilder)가 자기 처리를 등록. 카드는 누가 눌렸는지만 전달.
        private Action<DeckCard> _onTap;

        private void Awake()
        {
            if (_button == null) _button = GetComponent<Button>();
            _button.onClick.AddListener(() => _onTap?.Invoke(this));

            // 편성 중 오버레이 버튼 — 카드 버튼과 같은 콜백. 편성 중(오버레이 ON)일 때 이게 클릭 받음.
            //   보유 목록: 편성 중 재터치 → 해제(호출부 분기). 슬롯 안 카드: 오버레이 안 켜져 무관.
            if (_inDeckButton != null)
                _inDeckButton.onClick.AddListener(() => _onTap?.Invoke(this));
        }

        // PieceID로 비주얼 채움. 호출부는 이 한 줄 + 탭 콜백만.
        public void Setup(int pieceID, Action<DeckCard> onTap)
        {
            PieceID = pieceID;
            _onTap = onTap;

            var data = PieceQuery.Get(pieceID);
            if (data == null) return;

            if (_portrait != null) _portrait.sprite = PieceQuery.GetPortrait(pieceID);
            if (_frame != null) _frame.sprite = PieceQuery.GetCardFrame(pieceID);
            if (_background != null) _background.sprite = PieceQuery.GetCardBackground(pieceID);
            if (_typeIcon != null) _typeIcon.sprite = PieceQuery.GetTypeIcon(pieceID);
            if (_lvText != null) _lvText.text = $"Lv {data.PieceLv}";
            SetStars(data.PieceGrade);

            SetInDeck(false);  // 초기 — 편성 안 됨
        }

        // 편성 중 표시 토글 — 오버레이가 Raycast로 클릭을 흡수해 편성된 카드는 못 누름.
        // 켜는 즉시 이미지가 바뀌고 클릭이 막힘(탭바 ActiveImage 토글과 같은 방식).
        public void SetInDeck(bool inDeck)
        {
            if (_statusOverlay != null) _statusOverlay.SetActive(inDeck);
            if (_statusInDeck != null) _statusInDeck.SetActive(inDeck);
            if (_statusNotOwned != null) _statusNotOwned.SetActive(false); // 미보유 후순위
        }

        // 성급만큼 별 켜기 (1~3 고정, 별 3개 중 Grade개 ON)
        private void SetStars(int grade)
        {
            if (_stars == null) return;
            for (int i = 0; i < _stars.Length; i++)
                if (_stars[i] != null) _stars[i].SetActive(i < grade);
        }
    }
}
using System;
using InGame.Sort.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 캐릭터 상세보기 팝업 — 카드 Long Press로 열림. 유형·성급·이름·Lv·스탯·일러스트 표시.
    //   일러스트(감상) 모드 — 같은 버튼 토글로 정보 패널(StatInfo_Area)만 껐다 켬.
    //     일러스트·유형 아이콘·별·기물·버튼은 양쪽 공통. 팝업 제목은 고정.
    //   스탯은 연결 포탑(TowerData)에서 조회 — 기물 자체엔 전투 수치가 없음.
    //   편성하기 → onEquip 위임 후 닫힘. 취소 → 닫기. 미보유면 편성하기 비활성.
    //   딤은 PopupManager가 공용 관리 — 닫기는 PopupBase.Close.
    // ※ 단위 문구("성"·"발"·"s")는 코드에 박힘 — 로컬라이즈 시 손볼 지점.
    // 작성자: 이성규
    public class PieceDetailPopup : PopupBase
    {
        [Header("상단")]
        [SerializeField] private Image _typeIcon;
        [SerializeField] private GameObject[] _stars;      // Star_1~3, Grade만큼 ON

        [Tooltip("일러스트 모드 토글 — 정보 패널 표시/숨김 겸용")]
        [SerializeField] private Button _illustButton;

        [Header("본문")]
        [Tooltip("캐릭터 상세 일러스트 — 모드와 무관하게 항상 표시")]
        [SerializeField] private Image _detailIllust;

        [Tooltip("기물 일러스트 — Piece_Visual/Piece_Image")]
        [SerializeField] private Image _pieceIllust;

        [Header("정보 패널 (일러스트 모드에서 숨김)")]
        [Tooltip("StatInfo_Area — 이 오브젝트만 토글")]
        [SerializeField] private GameObject _infoPanel;

        [Tooltip("TopInfo/Name_Text")]
        [SerializeField] private TMP_Text _nameText;

        [Tooltip("TopInfo/Level_Text")]
        [SerializeField] private TMP_Text _levelText;

        [Header("스탯 (TowerData)")]
        [SerializeField] private TMP_Text _typeText;       // Type/Type_Info_Text
        [SerializeField] private TMP_Text _gradeText;      // Rairty/Type_Info_Text
        [SerializeField] private TMP_Text _damageText;     // Damage/Type_Info_Text
        [SerializeField] private TMP_Text _ammoText;       // Ammo/Type_Info_Text
        [SerializeField] private TMP_Text _rangeText;      // Range/Type_Info_Text
        [SerializeField] private TMP_Text _fireRateText;   // FireRate/Type_Info_Text

        [Header("버튼")]
        [SerializeField] private Button _equipButton;      // 편성하기
        [SerializeField] private Button _cancelButton;     // 취소

        private int _pieceID;
        private Action<int> _onEquip;
        private bool _illustMode;

        protected override void OnAwake()
        {
            if (_illustButton != null) _illustButton.onClick.AddListener(ToggleIllustMode);
            if (_equipButton != null) _equipButton.onClick.AddListener(OnTapEquip);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(Close);
        }

        // 열기 — pieceID의 정보로 채움. 편성하기 누르면 onEquip(pieceID).
        //   미보유면 편성하기 비활성(해금은 강화창).
        public void Show(int pieceID, bool isOwned, Action<int> onEquip)
        {
            _pieceID = pieceID;
            _onEquip = onEquip;

            var data = PieceQuery.Get(pieceID);
            if (data == null) return;

            // 비주얼
            if (_typeIcon != null) _typeIcon.sprite = PieceQuery.GetTypeIcon(pieceID);
            if (_detailIllust != null) _detailIllust.sprite = PieceQuery.GetDetailIllust(pieceID);
            if (_pieceIllust != null) _pieceIllust.sprite = PieceQuery.GetSprite(pieceID);
            SetStars(data.PieceGrade);

            // 이름·레벨 (분리 텍스트)
            if (_nameText != null) _nameText.text = data.PieceName;
            if (_levelText != null) _levelText.text = $"Lv. {data.PieceLv}";

            SetStats(pieceID, data.PieceGrade);

            if (_equipButton != null) _equipButton.interactable = isOwned;

            SetIllustMode(false);   // 항상 기본 모드(정보 패널 표시)로 시작
            Open();
        }

        // ---- 스탯 ----

        // 전투 수치는 연결 포탑에서. 포탑 데이터가 없으면 대시로 비움(빈 칸보다 낫다).
        private void SetStats(int pieceID, int grade)
        {
            if (_gradeText != null) _gradeText.text = $"{grade}성";

            var tower = PieceQuery.GetTower(pieceID);
            if (tower == null)
            {
                SetText(_typeText, "-");
                SetText(_damageText, "-");
                SetText(_ammoText, "-");
                SetText(_rangeText, "-");
                SetText(_fireRateText, "-");
                return;
            }

            SetText(_typeText, tower.TowerTypeText);
            SetText(_damageText, tower.TowerAtk.ToString());
            SetText(_ammoText, $"{tower.TowerMaxAmmo}발");
            SetText(_rangeText, tower.TowerMaxRange.ToString("0.##"));
            SetText(_fireRateText, $"{tower.TowerAtkSpeed:0.00}s");
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null) target.text = value;
        }

        // ---- 일러스트(감상) 모드 ----

        private void ToggleIllustMode() => SetIllustMode(!_illustMode);

        // 정보 패널만 토글 — 일러스트·유형·별·기물·버튼·제목은 양쪽 공통.
        private void SetIllustMode(bool on)
        {
            _illustMode = on;
            if (_infoPanel != null) _infoPanel.SetActive(!on);
        }

        // ---- 버튼 ----

        // 편성하기 — 위임 후 닫힘. 6칸이 찼는지 등 판단은 호출부(DeckBuilder).
        private void OnTapEquip()
        {
            _onEquip?.Invoke(_pieceID);
            Close();
        }

        private void SetStars(int grade)
        {
            if (_stars == null) return;
            for (int i = 0; i < _stars.Length; i++)
                if (_stars[i] != null) _stars[i].SetActive(i < grade);
        }
    }
}
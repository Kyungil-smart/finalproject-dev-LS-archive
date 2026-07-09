using System;
using InGame.Sort.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 캐릭터 상세보기 팝업 — 카드 Long Press로 열림. 유형·성급·이름·Lv·일러스트 표시.
    //   일러스트(감상) 모드 — 같은 버튼 토글로 정보 패널만 껐다 켬. 일러스트는 처음부터 상세본 고정.
    //   편성하기 → onEquip 위임 후 닫힘. 취소 → 닫기.
    //   스탯 표시는 기획 v0.5에서 제거됨(이름·Lv만).
    //   딤은 PopupManager가 공용 관리 — 닫기는 PopupBase.Close.
    // 작성자: 이성규
    public class PieceDetailPopup : PopupBase
    {
        [Header("상단")]
        [SerializeField] private Image _typeIcon;
        [SerializeField] private GameObject[] _stars;      // Grade만큼 ON

        [Tooltip("일러스트 모드 토글 — 정보 패널 표시/숨김 겸용")]
        [SerializeField] private Button _illustButton;

        [Header("본문")]
        [Tooltip("캐릭터 상세 일러스트 — 모드와 무관하게 항상 표시")]
        [SerializeField] private Image _detailIllust;

        [Tooltip("기물 일러스트")]
        [SerializeField] private Image _pieceIllust;

        [Tooltip("정보 패널 — 이름·Lv. 일러스트 모드에서 숨김")]
        [SerializeField] private GameObject _infoPanel;

        [SerializeField] private TMP_Text _nameText;       // "세린 Lv 10"

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

            if (_typeIcon != null) _typeIcon.sprite = PieceQuery.GetTypeIcon(pieceID);
            if (_detailIllust != null) _detailIllust.sprite = PieceQuery.GetDetailIllust(pieceID);
            if (_pieceIllust != null) _pieceIllust.sprite = PieceQuery.GetSprite(pieceID);
            if (_nameText != null) _nameText.text = $"{data.PieceName} Lv {data.PieceLv}";

            SetStars(data.PieceGrade);

            if (_equipButton != null) _equipButton.interactable = isOwned;

            SetIllustMode(false);   // 항상 기본 모드(정보 패널 표시)로 시작
            Open();
        }

        // ---- 일러스트(감상) 모드 ----

        private void ToggleIllustMode() => SetIllustMode(!_illustMode);

        // 정보 패널만 토글 — 일러스트·유형·별·기물·버튼은 양쪽 공통.
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
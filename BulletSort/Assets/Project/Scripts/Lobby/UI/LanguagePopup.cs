using System;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 언어 설정 팝업 — 한국어/영어 선택(옵션 텍스트 터치) + 변경/취소. 순수 표시(진입점은 PopupManager).
    //   선택된 언어의 화살표 묶음(KR_Arrows/EN_Arrows)만 SetActive로 표시.
    //   옵션 클릭은 임시 선택만 바꿈 — 실제 적용은 '변경' 버튼에서 콜백으로 위임. 취소는 적용 없이 닫기.
    //   실제 언어 전환(로컬라이즈)은 이월 과제 — 팝업은 선택 상태·화살표 토글만, 적용은 onApply 콜백으로.
    // 공통(root 토글·시작 비활성화·Close)은 PopupBase가 담당.
    // 작성자: 이성규
    public class LanguagePopup : PopupBase
    {
        [Header("Options")]
        [Tooltip("한국어 옵션 버튼")]
        [SerializeField] private Button _koreanButton;
        [Tooltip("한국어 선택 표시 화살표 묶음")]
        [SerializeField] private GameObject _koreanArrows;

        [Tooltip("영어 옵션 버튼")]
        [SerializeField] private Button _englishButton;
        [Tooltip("영어 선택 표시 화살표 묶음")]
        [SerializeField] private GameObject _englishArrows;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;   // 변경
        [SerializeField] private Button _cancelButton;    // 취소

        // 변경 확정 시 실제 적용 위임 — 로컬라이즈 로직이 이 콜백을 받아 처리(이월).
        private Action<Language> _onApply;

        // 팝업 열려 있는 동안의 임시 선택 — 변경 버튼 전까지 실제 적용 안 함.
        private Language _selected;

        protected override void OnAwake()
        {
            _koreanButton.onClick.AddListener(() => SetSelected(Language.KO));
            _englishButton.onClick.AddListener(() => SetSelected(Language.EN));
            _confirmButton.onClick.AddListener(OnConfirm);
            _cancelButton.onClick.AddListener(Close);
        }

        // 표시 — 현재 적용된 언어로 초기 선택 세팅, 변경 시 실행할 콜백 등록.
        public void Show(Language current, Action<Language> onApply)
        {
            _onApply = onApply;
            SetSelected(current);   // 현재 언어로 화살표 초기 표시
            Open();
        }

        // 임시 선택 변경 + 화살표 갱신 — 선택 기준으로 양쪽 한 번에 그려 둘 다 켜짐 방지.
        private void SetSelected(Language lang)
        {
            _selected = lang;
            if (_koreanArrows != null) _koreanArrows.SetActive(lang == Language.KO);
            if (_englishArrows != null) _englishArrows.SetActive(lang == Language.EN);
        }

        // 변경 — 선택된 언어로 적용 위임 후 닫기.
        private void OnConfirm()
        {
            var cb = _onApply;
            var lang = _selected;
            Close();            // 먼저 닫고 콜백
            cb?.Invoke(lang);
        }

        public override void Close()
        {
            base.Close();
            _onApply = null;
        }
    }
}
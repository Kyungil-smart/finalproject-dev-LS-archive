using System;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 언어 설정 팝업 — 한국어/영어/일본어 선택(옵션 터치) + 변경/취소. 순수 표시(진입점은 PopupManager).
    //   선택된 언어의 화살표 묶음만 SetActive로 표시(SetSelected가 셋을 한 번에 그려 중복 방지).
    //   옵션 클릭은 임시 선택만 바꿈 — 실제 적용은 '변경' 버튼에서 onApply 콜백으로 위임.
    //     콜백을 받은 SettingController가 LocalizationManager.SetLanguage로 Locale을 교체.
    //   취소는 적용 없이 닫기 — 임시 선택은 다음 Show에서 현재 언어로 덮임.
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

        [Tooltip("일본어 옵션 버튼")]
        [SerializeField] private Button _japaneseButton;
        [Tooltip("일본어 선택 표시 화살표 묶음")]
        [SerializeField] private GameObject _japaneseArrows;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;   // 변경
        [SerializeField] private Button _cancelButton;    // 취소

        // 변경 확정 시 적용 위임 — 팝업은 Locale을 직접 안 건드림.
        private Action<Language> _onApply;

        // 팝업이 열려 있는 동안의 임시 선택 — 변경 버튼 전까지 실제 적용 안 함.
        private Language _selected;

        protected override void OnAwake()
        {
            _koreanButton.onClick.AddListener(() => SetSelected(Language.KO));
            _englishButton.onClick.AddListener(() => SetSelected(Language.EN));
            _japaneseButton.onClick.AddListener(() => SetSelected(Language.JA));

            _confirmButton.onClick.AddListener(OnConfirm);
            _cancelButton.onClick.AddListener(Close);
        }

        // 표시 — 현재 적용된 언어로 초기 선택 세팅, 변경 시 실행할 콜백 등록.
        public void Show(Language current, Action<Language> onApply)
        {
            _onApply = onApply;
            SetSelected(current);
            Open();
        }

        // 임시 선택 변경 + 화살표 갱신 — 선택 기준으로 셋을 한 번에 그려 둘 이상 켜짐 방지.
        private void SetSelected(Language lang)
        {
            _selected = lang;
            if (_koreanArrows != null) _koreanArrows.SetActive(lang == Language.KO);
            if (_englishArrows != null) _englishArrows.SetActive(lang == Language.EN);
            if (_japaneseArrows != null) _japaneseArrows.SetActive(lang == Language.JA);
        }

        // 변경 — 닫은 뒤 콜백. Close()가 _onApply를 비우므로 지역 변수에 먼저 담아둠.
        private void OnConfirm()
        {
            var cb = _onApply;
            var lang = _selected;
            Close();
            cb?.Invoke(lang);
        }

        public override void Close()
        {
            base.Close();
            _onApply = null;
        }
    }
}
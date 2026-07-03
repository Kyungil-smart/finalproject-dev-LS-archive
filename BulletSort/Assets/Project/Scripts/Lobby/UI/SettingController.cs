using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 설정창 컨트롤러 — 언어·크레딧 버튼을 팝업(PopupManager)에 연결.
    //   현재 범위: 버튼 → 팝업 표시 연결만. 볼륨 슬라이더 연동은 사운드 매니저 개발 시로 이월.
    //   언어 적용(로컬라이즈)도 이월 — 변경 콜백 경로만 열어두고 지금은 로그만.
    // 작성자: 이성규
    public class SettingController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _languageButton;
        [SerializeField] private Button _creditButton;

        // 현재 선택 언어 — 로컬라이즈 붙기 전까지 임시 보관(기본 KO).
        //   TODO: 실제 언어 상태는 추후 사운드/로컬라이즈 매니저 등에서 관리하도록 이관.
        private Language _currentLanguage = Language.KO;

        private void Start()
        {
            if (_languageButton != null)
                _languageButton.onClick.AddListener(OnTapLanguage);
            if (_creditButton != null)
                _creditButton.onClick.AddListener(OnTapCredit);
        }

        // 언어 설정 팝업 — 현재 언어로 열고, 변경 시 OnLanguageApply로 위임.
        private void OnTapLanguage()
        {
            PopupManager.Instance.ShowLanguage(_currentLanguage, OnLanguageApply);
        }

        // 언어 변경 확정 콜백 — 추후 로컬라이즈 연결 지점.
        //   지금은 선택만 반영 + 로그. 실제 텍스트 전환은 로컬라이즈 구현 시 여기 채움.
        private void OnLanguageApply(Language lang)
        {
            _currentLanguage = lang;

            // TODO: 로컬라이즈 적용 — LocalizationManager.SetLanguage(lang) 등 연결.
            Debug.Log($"[SettingController] 언어 변경 선택: {lang} (실제 적용은 로컬라이즈 구현 후)");
        }

        // 만든 사람들 팝업 — 내용 고정, 열기만.
        private void OnTapCredit()
        {
            PopupManager.Instance.ShowCredit();
        }
    }
}
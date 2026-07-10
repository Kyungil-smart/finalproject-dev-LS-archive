using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 설정창 컨트롤러 — 언어·크레딧 버튼을 팝업(PopupManager)에 연결.
    //   언어는 팝업이 선택만 반환하고, 실제 Locale 교체·저장은 LocalizationManager가 담당.
    //   볼륨 슬라이더 연동은 사운드 매니저 개발 시로 이월.
    // 작성자: 이성규
    public class SettingController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _languageButton;
        [SerializeField] private Button _creditButton;

        // 현재 언어 — 팝업의 초기 선택 표시에 사용. 저장값은 Start에서 읽음.
        //   ※ 필드 초기화는 MonoBehaviour 생성자 시점이라 PlayerPrefs 접근 불가(UnityException).
        //     에디터에서 씬을 열기만 해도 터지므로 반드시 Start/Awake에서.
        private Language _currentLanguage = Language.KO;

        private void Start()
        {
            _currentLanguage = LocalizationManager.Current;

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

        // 언어 변경 적용 — LocalizationManager가 Locale 교체·저장까지 담당.
        private void OnLanguageApply(Language lang)
        {
            _currentLanguage = lang;
            LocalizationManager.SetLanguage(lang);
        }

        // 만든 사람들 팝업 — 내용 고정, 열기만.
        private void OnTapCredit()
        {
            PopupManager.Instance.ShowCredit();
        }
    }
}
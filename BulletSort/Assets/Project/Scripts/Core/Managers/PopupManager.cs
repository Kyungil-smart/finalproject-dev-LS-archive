using System;
using Lobby.UI;
using UnityEngine;

namespace Core
{
    // 팝업 진입점 — 씬 어디서든 PopupManager.Instance.ShowXxx(...)로 팝업 요청.
    //   각 팝업(Alert·Language·Credit 등)은 표시만 담당, 매니저가 참조·위임. 호출부는 매니저만 알면 됨.
    //   팝업 종류 늘면 여기 ShowXxx 메서드 + 참조 + OnClosed 구독 추가 — 호출부는 무변경.
    // 딤 배경 공용 관리 — 딤은 씬에 하나(Popup_Root 하위). 팝업마다 복붙 안 함.
    //   Show 경로에서 딤 켜고, 팝업이 닫히면(OnClosed) 딤 끔. 어느 경로로 닫혀도 딤 정리됨.
    //   ※ 현재 팝업은 하나씩만 뜨는 전제. 겹침(중첩)이 생기면 열린 수 카운트 방식으로 확장 필요.
    // 싱글톤 — 팝업 캔버스(최상위)에 미리 두고. parent 있어 DontDestroyOnLoad 안 걸림 = 로비 씬 전용.
    //   (인게임 팝업 공용화는 차후 — 현재 범위 밖)
    // 작성자: 이성규
    public class PopupManager : Singleton<PopupManager>
    {
        [Header("Common")]
        [Tooltip("공용 딤 배경 — 씬에 하나. 팝업 열리면 켜지고 닫히면 꺼짐(매니저가 관리)")]
        [SerializeField] private GameObject _dimBackground;

        [Header("Popups")]
        [Tooltip("알림 팝업 — 제목·메시지·확인(+취소)")]
        [SerializeField] private AlertPopup _alertPopup;

        [Tooltip("언어 설정 팝업 — 한국어/영어 선택")]
        [SerializeField] private LanguagePopup _languagePopup;

        [Tooltip("만든 사람들(크레딧) 팝업")]
        [SerializeField] private CreditPopup _creditPopup;

        // 각 팝업의 닫힘을 구독 — 어느 팝업이 닫히든 딤 정리. (Awake 대신 Singleton의 Init)
        protected override void Init()
        {
            if (_alertPopup != null) _alertPopup.OnClosed += HideDim;
            if (_languagePopup != null) _languagePopup.OnClosed += HideDim;
            if (_creditPopup != null) _creditPopup.OnClosed += HideDim;

            HideDim();   // 시작은 딤 꺼짐
        }

        // ---- 알림 ----

        // 알림 — 확인 단일. 메시지만(제목 "알림" 기본).
        public void ShowAlert(string message)
        {
            if (!HasAlert()) return;
            ShowDim();
            _alertPopup.Show(message);
        }

        // 알림 — 확인 + 콜백.
        public void ShowAlert(string message, Action onConfirm)
        {
            if (!HasAlert()) return;
            ShowDim();
            _alertPopup.Show(message, onConfirm);
        }

        // 알림 — 확인+취소 이중. 취소 콜백 있으면 취소 버튼 ON.
        public void ShowAlert(string message, Action onConfirm, Action onCancel)
        {
            if (!HasAlert()) return;
            ShowDim();
            _alertPopup.Show(message, onConfirm, onCancel);
        }

        // 알림 — 제목까지 지정(ERROR·나가기 등 "알림" 외).
        public void ShowAlert(string title, string message, Action onConfirm, Action onCancel)
        {
            if (!HasAlert()) return;
            ShowDim();
            _alertPopup.Show(title, message, onConfirm, onCancel);
        }

        // ---- 언어 설정 ----

        // 언어 팝업 — 현재 언어로 초기 표시, 변경 시 onApply로 실제 적용 위임(로컬라이즈는 이월).
        public void ShowLanguage(Language current, Action<Language> onApply)
        {
            if (!HasLanguage()) return;
            ShowDim();
            _languagePopup.Show(current, onApply);
        }

        // ---- 크레딧 ----

        // 만든 사람들 팝업 — 내용 고정, 열기만.
        public void ShowCredit()
        {
            if (!HasCredit()) return;
            ShowDim();
            _creditPopup.Show();
        }

        // ---- 공용 딤 ----

        private void ShowDim()
        {
            if (_dimBackground != null) _dimBackground.SetActive(true);
        }

        private void HideDim()
        {
            if (_dimBackground != null) _dimBackground.SetActive(false);
        }

        // ---- 연결 확인 가드 — 미연결이면 경고 후 false. 조용한 실패(인스펙터 빠뜨림) 방지. ----

        private bool HasAlert()
        {
            if (_alertPopup != null) return true;
            Debug.LogWarning("[PopupManager] _alertPopup 미연결 — 인스펙터에서 AlertPopup 참조 연결 필요.");
            return false;
        }

        private bool HasLanguage()
        {
            if (_languagePopup != null) return true;
            Debug.LogWarning("[PopupManager] _languagePopup 미연결 — 인스펙터에서 LanguagePopup 참조 연결 필요.");
            return false;
        }

        private bool HasCredit()
        {
            if (_creditPopup != null) return true;
            Debug.LogWarning("[PopupManager] _creditPopup 미연결 — 인스펙터에서 CreditPopup 참조 연결 필요.");
            return false;
        }
    }
}
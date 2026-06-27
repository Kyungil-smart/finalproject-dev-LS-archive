using System;
using Lobby.UI;
using UnityEngine;

namespace Core
{
    // 팝업 진입점 — 씬 어디서든 PopupManager.Instance.ShowAlert(...)로 팝업 요청.
    //   각 팝업(AlertPopup 등)은 표시만 담당, 매니저가 참조·위임. 호출부는 매니저만 알면 됨.
    //   팝업 종류 늘면 여기 ShowXxx 메서드 추가(ShowConfirm·ShowLoading 등) — 호출부는 무변경.
    // 싱글톤 — 팝업 캔버스(최상위)에 미리 두고. parent 있어 DontDestroyOnLoad 안 걸림 = 로비 씬 전용.
    //   (인게임 팝업 공용화는 차후 — 현재 범위 밖)
    // 작성자: 이성규
    public class PopupManager : Singleton<PopupManager>
    {
        [Header("Popups")]
        [Tooltip("알림 팝업 — 제목·메시지·확인(+취소). 같은 캔버스 아래 미리 둠")]
        [SerializeField] private AlertPopup _alertPopup;

        // 알림 — 확인 단일. 메시지만(제목 "알림" 기본).
        public void ShowAlert(string message)
        {
            if (!HasAlert()) return;
            _alertPopup.Show(message);
        }

        // 알림 — 확인 + 콜백.
        public void ShowAlert(string message, Action onConfirm)
        {
            if (!HasAlert()) return;
            _alertPopup.Show(message, onConfirm);
        }

        // 알림 — 확인+취소 이중. 취소 콜백 있으면 취소 버튼 ON.
        public void ShowAlert(string message, Action onConfirm, Action onCancel)
        {
            if (!HasAlert()) return;
            _alertPopup.Show(message, onConfirm, onCancel);
        }

        // 알림 — 제목까지 지정(ERROR·나가기 등 "알림" 외).
        public void ShowAlert(string title, string message, Action onConfirm, Action onCancel)
        {
            if (!HasAlert()) return;
            _alertPopup.Show(title, message, onConfirm, onCancel);
        }

        // 알림 팝업 연결 확인 — 미연결이면 경고 후 false. 조용한 실패(인스펙터 빠뜨림) 방지.
        private bool HasAlert()
        {
            if (_alertPopup != null) return true;
            Debug.LogWarning("[PopupManager] _alertPopup 미연결 — 인스펙터에서 AlertPopup 참조 연결 필요.");
            return false;
        }
    }
}
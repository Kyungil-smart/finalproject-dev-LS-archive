using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 알림 팝업 표시 컴포넌트 — 제목·메시지·확인(+취소) 버튼 토글. 순수 표시만(진입점은 PopupManager).
    //   피그마 알림 팝업 12종이 같은 골격(제목+메시지+버튼 1~2개)이라 하나로 공용 처리.
    //   호출은 PopupManager.Instance.ShowAlert(...)를 통함 — 직접 Show 호출 안 함(매니저가 참조·위임).
    // 버튼 — 취소 콜백 유무로 단일/이중 분기. 취소 콜백 없으면 취소 버튼 OFF(확인 단일).
    //   버튼 영역은 HorizontalLayoutGroup — 취소 SetActive 토글이 레이아웃 자동 재배치.
    //   메시지 줄바꿈은 호출부가 \n으로(예: "...부족합니다.\n6개를...").
    // 작성자: 이성규
    public class AlertPopup : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("팝업 루트 — Show/Hide로 토글(이 컴포넌트가 붙은 오브젝트와 별개일 수 있어 분리)")]
        [SerializeField] private GameObject _root;

        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;   // 이중일 때만 ON

        private const string DefaultTitle = "알림";

        // 버튼별 1회용 콜백 — Show마다 갱신. 누르면 닫고 콜백 실행.
        private Action _onConfirm;
        private Action _onCancel;

        private void Awake()
        {
            if (_root == null) _root = gameObject;

            _confirmButton.onClick.AddListener(OnConfirm);
            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancel);

            _root.SetActive(false);   // 시작은 닫힌 상태
        }

        // 확인 단일 — 제목 "알림", 메시지만. 확인 누르면 닫기.
        public void Show(string message) => Show(DefaultTitle, message, null, null);

        // 확인 단일 + 콜백 — 확인 누르면 콜백 후 닫기.
        public void Show(string message, Action onConfirm) => Show(DefaultTitle, message, onConfirm, null);

        // 확인+취소 이중 — 취소 콜백 있으면 취소 버튼 ON.
        public void Show(string message, Action onConfirm, Action onCancel)
            => Show(DefaultTitle, message, onConfirm, onCancel);

        // 전체 지정 — 제목까지(ERROR·나가기 등 "알림" 외).
        public void Show(string title, string message, Action onConfirm, Action onCancel)
        {
            if (_titleText != null) _titleText.text = title;
            if (_messageText != null) _messageText.text = message;

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            // 취소 콜백 유무로 단일/이중 — 취소 버튼 토글.
            if (_cancelButton != null)
                _cancelButton.gameObject.SetActive(onCancel != null);

            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
            _onConfirm = null;
            _onCancel = null;
        }

        private void OnConfirm()
        {
            var cb = _onConfirm;
            Hide();          // 먼저 닫고 콜백 — 콜백이 또 팝업 띄워도 상태 안 꼬이게
            cb?.Invoke();
        }

        private void OnCancel()
        {
            var cb = _onCancel;
            Hide();
            cb?.Invoke();
        }
    }
}
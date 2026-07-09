using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 공격 유형 보기 팝업 — 내용 고정(6종 유형 설명 정지 비주얼), 열기만.
    //   덱 편성 보유 목록의 i 버튼 → PopupManager.Instance.ShowAttackType().
    //   딤은 PopupManager가 공용 관리 — 이 팝업은 표시·닫힘 알림만 담당.
    // 작성자: 이성규
    public class AttackTypePopup : PopupBase
    {
        [Header("Buttons")]
        [Tooltip("확인 버튼 — 닫기")]
        [SerializeField] private Button _confirmButton;

        private void Awake()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(Close);   // PopupBase.Close (OnClosed 발행 → 딤 정리)
        }

        // 열기 — 내용 고정이라 인자 없음. (CreditPopup과 같은 결)
        public void Show()
        {
            Open();
        }
    }
}
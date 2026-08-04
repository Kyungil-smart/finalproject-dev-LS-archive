using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 만든 사람들(크레딧) 팝업 — 크레딧 목록 표시 + 확인 버튼. 순수 표시(진입점은 PopupManager).
    //   크레딧 텍스트는 프리팹에 고정(스크롤 에어리어). 여기선 열고 닫기만.
    //   공통(root 토글·시작 비활성화·Close)은 PopupBase가 담당.
    // 작성자: 이성규
    public class CreditPopup : PopupBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;

        protected override void OnAwake()
        {
            if(_confirmButton != null)
                _confirmButton.onClick.AddListener(Close);
        }
        
        // 표시 — 내용 고정이라 열기만.
        public void Show() => Open();
    }
}

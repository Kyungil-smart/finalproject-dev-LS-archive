using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Deck
{
    // 공격 유형 보기 — 보유 목록의 i 버튼으로 열고 확인 버튼으로 닫음.
    //   내용은 정지 비주얼(6종 유형 설명)이라 데이터 바인딩 없음. 튜토리얼 2-6 비주얼 재활용.
    //   PopupManager 경유 안 함 — 딤·스택이 필요 없는 단순 토글.
    // 작성자: 이성규
    public class AttackTypeInfoPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;        // AttackType_Info 루트 (기본 꺼짐)
        [SerializeField] private Button _openButton;       // i 버튼
        [SerializeField] private Button _closeButton;      // 확인 버튼

        private void Awake()
        {
            if (_openButton != null) _openButton.onClick.AddListener(Open);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);

            if (_panel != null) _panel.SetActive(false);   // 시작은 닫힘
        }

        private void Open()
        {
            if (_panel != null) _panel.SetActive(true);
        }

        private void Close()
        {
            if (_panel != null) _panel.SetActive(false);
        }
    }
}
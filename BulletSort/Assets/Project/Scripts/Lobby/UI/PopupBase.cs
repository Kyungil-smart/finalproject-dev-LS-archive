using System;
using UnityEngine;

namespace Lobby.UI
{
    // 팝업 공통 베이스 — root 토글·Close·시작 비활성화를 담당. 순수 표시(진입점은 PopupManager).
    //   각 팝업(Alert·Language·Credit 등)은 이 클래스를 상속하고, 자기 고유의 Show(...)를 정의.
    //   공통: _root SetActive 토글, 시작 시 닫힘, Close, 닫힘 통지. 고유: 표시 내용·버튼 콜백.
    // Awake는 이 클래스가 처리(_root 세팅·시작 비활성화). 자식은 추가 초기화가 필요하면 OnAwake 오버라이드.
    // 딤 배경은 팝업이 직접 안 들고 있음 — 닫힘을 OnClosed로 알리면 PopupManager가 공용 딤을 토글.
    //   (팝업마다 딤 복붙 방지 — 딤은 씬에 하나, 관리는 매니저)
    // 작성자: 이성규
    public abstract class PopupBase : MonoBehaviour
    {
        [Header("Popup Base")]
        [Tooltip("팝업 루트 — Open/Close로 토글(이 컴포넌트가 붙은 오브젝트와 별개일 수 있어 분리)")]
        [SerializeField] protected GameObject _root;

        // 닫힘 통지 — 매니저가 구독해서 공용 딤 배경 등 공통 후처리. 어느 경로로 닫히든(확인·취소·외부) 발행.
        public event Action OnClosed;

        protected virtual void Awake()
        {
            if (_root == null) _root = gameObject;
            OnAwake();
            _root.SetActive(false);   // 시작은 닫힌 상태
        }

        // 자식 추가 초기화 훅 — 버튼 리스너 등록 등. _root 비활성화 전에 호출됨.
        protected virtual void OnAwake() { }

        // 루트 열기 — 표시 내용 세팅 후 자식 Show(...)에서 호출. (딤은 매니저가 Show 경로에서 켬)
        protected void Open() => _root.SetActive(true);

        // 루트 닫기 — 공통. 닫힘을 OnClosed로 통지(매니저가 딤 정리).
        //   자식이 콜백 정리 등 추가 처리가 필요하면 오버라이드 후 base.Close() 호출.
        public virtual void Close()
        {
            _root.SetActive(false);
            OnClosed?.Invoke();
        }
    }
}
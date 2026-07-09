using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lobby.Deck
{
    // 길게 누르기 감지 — 카드에 붙여 Long Press 시 콜백. 짧게 떼면 일반 탭(Button이 처리).
    //   누른 채 _threshold초 지나면 발동. 그 후 손을 떼도 Button.onClick은 그대로 발화하므로
    //   DeckCard가 Consumed 플래그로 탭 처리를 거른다(탭=편성, 롱프레스=상세보기).
    //   스크롤뷰 안이라 OnPointerExit로 취소 — 누른 채 드래그하면 오발동 방지.
    // 작성자: 이성규
    public class LongPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Tooltip("길게 누르기 판정 시간(초)")]
        [SerializeField] private float _threshold = 0.5f;

        // 길게 눌렸을 때 발행 — DeckCard가 구독.
        public event Action OnLongPress;

        // 이번 누름이 롱프레스로 소비됐는지 — 뗄 때 탭 처리를 막는 용도.
        public bool Consumed { get; private set; }

        private float _downTime;
        private bool _pressing;

        public void OnPointerDown(PointerEventData e)
        {
            _pressing = true;
            Consumed = false;
            _downTime = Time.unscaledTime;
        }

        public void OnPointerUp(PointerEventData e) => _pressing = false;

        // 손가락이 카드 밖으로 나가면 취소(스크롤 중 오발동 방지)
        public void OnPointerExit(PointerEventData e) => _pressing = false;

        private void Update()
        {
            if (!_pressing || Consumed) return;

            if (Time.unscaledTime - _downTime >= _threshold)
            {
                Consumed = true;    // 이번 누름은 롱프레스로 소비 — 탭 무시
                _pressing = false;
                OnLongPress?.Invoke();
            }
        }
    }
}
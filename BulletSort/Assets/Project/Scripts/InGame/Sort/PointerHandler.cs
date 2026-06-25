using Core;
using UnityEngine;
using Logger = Core.Logger;

namespace InGame.Sort
{
    // 드래그 핸들링 담당 클래스.
    // InputManager의 입력 이벤트를 받아 Piece 레이어 raycast로 IDraggable 대상을 찾고,
    // 잡은 대상에게 OnGrabbed / OnDragging / OnReleased를 위임한다.
    // 본 클래스는 IDraggable 인터페이스만 알며, 구체 타입(Piece 등)은 모른다.
    // 작성자: 이성규
    public class PointerHandler : MonoBehaviour
    {
        // raycast 대상 레이어 — Piece 레이어(6번) 등을 인스펙터에서 지정.
        [SerializeField] private LayerMask _draggableLayer;

        // 현재 잡혀 있는 드래그 대상. 잡힌 게 없으면 null.
        private IDraggable _draggedTarget;

        private void Start()
        {
            // InputManager는 코어 매니저로 자동 생성됨 — 인스펙터 참조 불필요.
            if (InputManager.Instance == null)
            {
                Logger.Instance?.LogError("PointerHandler: InputManager.Instance 없음");
                return;
            }

            InputManager.Instance.OnPointerDown += OnPointerDown;
            InputManager.Instance.OnPointerDrag += OnPointerDrag;
            InputManager.Instance.OnPointerUp += OnPointerUp;
        }

        private void OnDestroy()
        {
            // InputManager가 먼저 파괴됐을 가능성 (씬 종료 순서 보장 안 됨)
            if (InputManager.Instance == null) return;

            InputManager.Instance.OnPointerDown -= OnPointerDown;
            InputManager.Instance.OnPointerDrag -= OnPointerDrag;
            InputManager.Instance.OnPointerUp -= OnPointerUp;
        }

        private void OnPointerDown(Vector2 worldPos)
        {
            // 이미 무언가 잡고 있는 상태에서 다른 입력이 들어왔다면 무시.
            // (멀티터치 두 번째 무시 — #3 기획서 2.4.3 예외 처리와 정합)
            if (_draggedTarget != null) return;

            // Piece 레이어 raycast로 잡힌 콜라이더 탐색.
            Collider2D hit = Physics2D.OverlapPoint(worldPos, _draggableLayer);
            if (hit == null) return;

            // 잡힌 콜라이더가 IDraggable을 구현했는지 확인.
            if (hit.TryGetComponent<IDraggable>(out var target))
            {
                // 잡기 차단 — 락 슬롯의 기물 등 CanGrab false면 무시(터치 자체 차단)
                if (!target.CanGrab) return;
                
                _draggedTarget = target;
                _draggedTarget.OnGrabbed(worldPos);
            }
        }

        private void OnPointerDrag(Vector2 worldPos)
        {
            // 잡힌 게 있을 때만 드래그 전달.
            _draggedTarget?.OnDragging(worldPos);
        }

        private void OnPointerUp(Vector2 worldPos)
        {
            if (_draggedTarget == null) return;

            _draggedTarget.OnReleased(worldPos);
            _draggedTarget = null;
        }
    }
}
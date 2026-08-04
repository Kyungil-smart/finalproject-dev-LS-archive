using Core;
using InGame.Slot;
using UnityEngine;
using Logger = Core.Logger;

namespace InGame.Sort
{
    // 드래그 핸들링 — InputManager 입력을 받아 SlotBoardManager 최근접 기물을 잡고,
    //   OnGrabbed/OnDragging/OnReleased를 IDraggable에 위임. 구체 타입(Piece 등)은 모름.
    //   잡기 판정을 콜라이더 OverlapPoint → 최근접 셀 기물로 변경(손가락 오차·가림 흡수).
    // 작성자: 이성규
    public class PointerHandler : MonoBehaviour
    {
        [Header("Grab")]
        [Tooltip("잡기 시 최근접 기물 스냅 허용 반경(월드). 이보다 멀면 잡기 안 됨. 해상도별로 튜닝")]
        [SerializeField] private float _grabMaxDistance = 2f;

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
            if (SlotBoardManager.Instance == null) return;

            // 최근접 기물 잡기 — 락 슬롯 기물(CanGrab false)이면 무시.
            IDraggable target = SlotBoardManager.Instance.FindNearestGrabbable(worldPos, _grabMaxDistance);
            if (target == null || !target.CanGrab) return;
            
            _draggedTarget = target;
            _draggedTarget.OnGrabbed(worldPos);
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
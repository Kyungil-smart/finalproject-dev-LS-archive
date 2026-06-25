namespace Core
{
    // 드래그 가능한 오브젝트의 계약.
    // PointerHandler가 이 인터페이스를 통해서만 대상과 상호작용 — 구체 타입(Piece 등)은 모름.
    // 향후 다른 드래그 대상(카메라·UI 등)이 추가되어도 같은 패턴으로 확장 가능.
    public interface IDraggable
    {
        // 잡기 가능 여부 — PointerHandler가 OnGrabbed 호출 전에 확인.
        //   false면 잡기 자체를 안 함(예: 슬롯 락 중인 기물). 터치 차단을 진입점에서 처리.
        bool CanGrab { get; }
        
        // 잡힌 순간 — 현재 포인터 월드 좌표 전달.
        void OnGrabbed(UnityEngine.Vector2 worldPos);

        // 드래그 중 매 프레임 — 현재 포인터 월드 좌표 전달.
        void OnDragging(UnityEngine.Vector2 worldPos);

        // 놓인 순간 — 현재 포인터 월드 좌표 전달.
        void OnReleased(UnityEngine.Vector2 worldPos);
    }
}
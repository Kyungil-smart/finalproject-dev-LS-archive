namespace InGame.Slot
{
    // 슬롯 표시 모드 — SlotState + 포탑 보유(Active/Queue)의 조합으로 *어떻게 그릴지* 결정.
    // 정렬 시 동작(연출·HP회복·탄환소모)과는 다른 축 — 여긴 순수 표시.
    // 작성자: 이성규
    public enum SlotDisplayMode
    {
        Normal,                  // 일반 슬롯 UI, HP바, 피격 연출
        NormalWithActive,        // + 가동 포탑 테두리·아이콘·잔탄
        NormalWithActiveQueue,   // + 대기열 포탑 아이콘까지
        Destroyed,               // 파괴 — Destroyed UI
    }

    // 포탑 보유 상태 입력 — 슬롯측 SlotTurretQueue가 구현해 컨트롤러에 주입.
    //   큐가 가동/대기 포탑 참조를 들고 HasActive/HasQueue로 보유 여부를, TowerType으로 종류를 보고.
    // 컨트롤러가 큐를 직접 참조하지 않도록 인터페이스로 끊음(결합 회피).
    // 미주입(null) 시 컨트롤러는 포탑 없음으로 간주 — Normal/Destroyed만 동작.
    //   타입 값은 표시용 int — 컨트롤러가 포탑 객체(ITower/Towers)를 모르게. 캐스팅은 큐 안에 가둠.
    public interface ITurretPresence
    {
        bool HasActiveTurret { get; }
        bool HasQueueTurret { get; }
        
        // 가동/대기 포탑 타입(TowerType 1~6) — 프레임·아이콘 스프라이트 선택용. 포탑 없으면 0.
        int ActiveTowerType { get; }
        int QueueTowerType { get; }
    }
}
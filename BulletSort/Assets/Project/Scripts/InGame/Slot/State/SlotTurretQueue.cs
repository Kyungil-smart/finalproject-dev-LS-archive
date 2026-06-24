using Towers.Interface.Tower;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯측 포탑 대기열 — 가동 포탑 1 + 대기 포탑 1을 참조로 보유(대기 최대 1, Queue 아닌 단일 참조).
    // 포탑 생성·탄환·교체·빠른소모는 포탑소환 시스템(안정연) 영역. 여긴 "누가 가동/대기인가"만 보유.
    //   비주얼·잔탄에 필요한 값(타입·탄)은 보유 포탑의 TowerInfo 구조체에서 직접 읽음 — 등록 시 따로 안 넘김.
    // ITurretPresence를 구현해 SlotDisplayController에 자신을 주입 — 표시 모드(Normal/+Active/+ActiveQueue) 판정에 사용.
    // 파괴 통보는 슬롯이 보유(합의 #3) — 포탑이 OnDestroy에서 NotifyTurretDestroyed(this) 호출.
    //   (ITower는 인터페이스 참조라 Unity-null 안전망이 없음 → OnDestroy 통보는 선택 아님 필수)
    // 작성자: 이성규
    public class SlotTurretQueue : MonoBehaviour, ITurretPresence
    {
        [Header("References")]
        [Tooltip("표시 컨트롤러 — 큐 변화 시 모드 재판정 대상. 비우면 Awake에서 탐색")]
        [SerializeField] private SlotDisplayController _display;
        
        // 가동/대기 포탑 — ITower 객체 참조(타입/ID 아닌 생성된 객체).
        // 대기 최대 1이라 컬렉션 불필요 — 단일 참조 2개로 충분.
        private ITower _active;
        private ITower _queue;
        
        // 표시 모드 판정 입력(ITurretPresence) — SlotDisplayController가 읽음.
        public bool HasActiveTurret => _active != null;
        public bool HasQueueTurret  => _queue  != null;
        
        // 외부(포탑 시스템·디버그·UI) 조회용 — 비주얼·잔탄은 여기서 TowerInfo를 읽어 처리.
        public ITower ActiveTurret => _active;
        public ITower QueueTurret  => _queue;

        private void Awake()
        {
            if (_display == null)
                _display = GetComponent<SlotDisplayController>()
                           ?? GetComponentInChildren<SlotDisplayController>(includeInactive: true);
        }

        private void Start()
        {
            // 표시 컨트롤러에 자신을 주입 — 이후 컨트롤러가 HasActive/HasQueue로 모드 판정.
            // SetTurretPresence가 내부에서 Refresh까지 호출.
            _display?.SetTurretPresence(this);
        }
        
        // 포탑 등록 — 생성 측이 생성 직후 호출.
        //   가동 비어있음     → 가동
        //   가동만 있음       → 대기
        //   가동·대기 모두 참 → 회전: 대기 승격→가동, 새 포탑→대기, 밀려난 가동 반환.
        //     반환된 포탑의 빠른소모·파괴는 호출부가 처리. 큐에선 이미 빠졌으니
        //     그 포탑이 나중에 파괴돼 통보가 와도 중복 승격 없음.
        // 반환: 회전 시 밀려난 기존 가동, 그 외 null.
        public ITower RegisterTurret(ITower turret)
        {
            if (turret == null) return null;
 
            if (_active == null)
            {
                _active = turret;
                RefreshVisual();
                return null;
            }
 
            if (_queue == null)
            {
                _queue = turret;
                RefreshVisual();
                return null;
            }
 
            // 회전 — 대기 승격 + 새 포탑 대기 등록 + 기존 가동 밀어냄.
            var pushedOut = _active;
            _active = _queue;
            _queue  = turret;
            RefreshVisual();
            return pushedOut;
        }
        
        // 파괴 통보 — 포탑이 OnDestroy에서 호출(슬롯이 보유).
        //   가동 파괴 → 대기를 가동으로 승격, 대기 비움.
        //   대기 파괴 → 대기만 비움. 그 외(이미 밀려난 포탑 등) → no-op.
        public void NotifyTurretDestroyed(ITower turret)
        {
            if (turret == null) return;
 
            if (turret == _active)
            {
                _active = _queue;   // 대기 승격(없으면 null)
                _queue  = null;
            }
            else if (turret == _queue)
            {
                _queue = null;
            }
            else
            {
                return;   // 이 슬롯의 가동·대기 아님 — 무시
            }
 
            RefreshVisual();
        }
        
        // 슬롯 파괴(HP 0) 일괄 정리용 — 가동·대기 모두 비움.
        // 호출 주체·시점은 협의 중. 정상은 포탑이 각자 파괴 통보로 비워짐 — 보조 진입점.
        public void ClearAll()
        {
            _active = null;
            _queue  = null;
            RefreshVisual();
        }
        
        // 큐 변화 → 표시 갱신.
        //   모드(Normal/+Active/+ActiveQueue)는 컨트롤러가 HasActive/HasQueue로 재판정.
        //   아이콘·테두리·잔탄은 UI 요소 신규 확정 후 연결(현재 미존재) — 아래 TODO.
        private void RefreshVisual()
        {
            _display?.Refresh();
            
            if (_active is Towers.Factory.Towers active)
            {
                int cur = active.CurrentAmmo;             // Towers에 이미 열림
                int max = active.TowerInfo.TowerMaxAmmo;  // 구조체 게터로 읽힘
                // _ammoBar.Set(cur, max);  // UI 요소 붙은 뒤
            }
 
            // TODO(UI 요소 신규 확정 후): 보유 포탑의 TowerInfo에서 읽어 표시.
            //   - 잔탄 바 ← (_active as Towers).CurrentAmmo / .TowerInfo.TowerMaxAmmo  (using Towers.Factory)
            //   - ActiveTurretIcon / QueueTurretIcon ← 포탑 타입
            //       ※ 타입은 현재 STowerInfo에 없음(TowerMaxAmmo 등만 노출) — 아이콘 작업 시 안정연이 TowerType 노출 필요.
            //   - 가동 테두리 on/off ← HasActiveTurret
        }
    }
}
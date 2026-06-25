using Towers.Interface.Tower;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯측 포탑 대기열 — 가동 포탑 1 + 대기 포탑 1을 참조로 보유(대기 최대 1, Queue 아닌 단일 참조).
    // 포탑 생성·탄환·교체·빠른소모는 포탑소환 시스템(포탑 담당) 영역. 여긴 "누가 가동/대기인가"만 보유.
    //   비주얼·잔탄에 필요한 값(타입·탄)은 보유 포탑의 TowerInfo 구조체에서 직접 읽음 — 등록 시 따로 안 넘김.
    // ITurretPresence를 구현해 SlotDisplayController에 자신을 주입 — 표시 모드(Normal/+Active/+ActiveQueue) 판정에 사용.
    // 파괴 통보는 슬롯이 보유(합의 #3) — 포탑이 OnDestroy에서 NotifyTurretDestroyed(this) 호출.
    //   (ITower는 인터페이스 참조라 Unity-null 안전망이 없음 → OnDestroy 통보는 선택 아님 필수)
    // 오버소팅(잔탄 과열 사격) 메커니즘은 SlotOverSorting으로 분리 — 큐는 "둘 다 찼다" 판정과
    //   최종 동작(승격·등록·정리)만, "어떻게(탄배출·락·pending)"는 위임. 큐가 여전히 사령탑.
    // 작성자: 이성규
    public class SlotTurretQueue : MonoBehaviour, ITurretPresence
    {
        [Header("References")]
        [Tooltip("표시 컨트롤러 — 큐 변화 시 모드 재판정 대상. 비우면 Awake에서 탐색")]
        [SerializeField] private SlotDisplayController _display;

        // 오버소팅 메커니즘 — 탄배출·락·pending 위임. 일반 C# 클래스라 큐가 직접 new로 소유(씬에 컴포넌트로 안 붙임).
        private readonly SlotOverSorting _overSorting = new SlotOverSorting();

        // 가동/대기 포탑 — ITower 객체 참조(타입/ID 아닌 생성된 객체).
        // 대기 최대 1이라 컬렉션 불필요 — 단일 참조 2개로 충분.
        private ITower _active;
        private ITower _queue;

        // 표시 모드 판정 입력(ITurretPresence) — SlotDisplayController가 읽음.
        public bool HasActiveTurret => _active != null;
        public bool HasQueueTurret  => _queue  != null;

        // 가동/대기 포탑 타입(TowerType 1~6) — 컨트롤러가 프레임·아이콘 스프라이트 선택에 사용.
        //   포탑 객체에서 타입을 꺼내는 캐스팅은 큐 안에 가둠(컨트롤러는 int만 받음). 없으면 0.
        public int ActiveTowerType => GetTowerType(_active);
        public int QueueTowerType  => GetTowerType(_queue);

        // 가동 포탑 잔탄(현재/최대) — 잔탄보드 표시용. 잔탄 관리는 포탑 영역, 큐는 읽어 노출만.
        //   포탑 캐스팅은 GetAmmo 헬퍼에 가둠(컨트롤러·보드는 int만). 가동 없으면 0.
        public int ActiveAmmoCurrent => _active is Towers.Factory.Towers t ? t.CurrentAmmo : 0;
        public int ActiveAmmoMax     => _active is Towers.Factory.Towers t ? t.TowerInfo.TowerMaxAmmo : 0;

        // 외부(포탑 시스템·디버그·UI) 조회용 — 비주얼·잔탄은 여기서 TowerInfo를 읽어 처리.
        public ITower ActiveTurret => _active;
        public ITower QueueTurret  => _queue;

        // 슬롯 락 — 오버소팅 상태를 그대로 패스스루. 슬롯 입력/정렬이 이걸 보고 차단.
        public bool IsLocked => _overSorting.IsLocked;

        // 포탑 객체 → TowerType(1~6). ITower엔 타입 seam이 없어 Towers 캐스팅 후 TowerInfo에서 읽음.
        //   포탑 담당이 ITower에 TowerType 노출하면 캐스팅 제거. 없거나 캐스팅 실패 시 0.
        private static int GetTowerType(ITower turret)
        {
            return turret is Towers.Factory.Towers t ? t.TowerInfo.TowerType : 0;
        }

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
        //   가동·대기 모두 참 → 오버소팅 진입(판정은 여기, 탄배출·락은 SlotOverSorting).
        //     기존 가동은 슬롯에 남은 채(=_active 유지) 잔탄 토함 → 소진 자가소멸 → 통보에서 마무리.
        // 반환: 없음(회전은 지연 — 즉시 밀어내지 않음).
        public void RegisterTurret(ITower turret)
        {
            if (turret == null) return;

            // 오버소팅 중엔 추가 등록 거부 — 락 규칙(추가 오버소팅 차단). 정상이면 입력단에서 이미 막힘.
            if (_overSorting.IsActive)
            {
                Debug.LogWarning("[SlotTurretQueue] 오버소팅 중 RegisterTurret 호출 — 무시. 입력 락 누락 확인.");
                return;
            }

            if (_active == null)
            {
                _active = turret;
                RefreshVisual();
                return;
            }

            if (_queue == null)
            {
                _queue = turret;
                RefreshVisual();
                return;
            }

            // 둘 다 찼다 — 판정은 큐가, 진입(탄배출·락·pending 보관)은 오버소팅에 위임.
            //   기존 가동(_active)은 그대로 두고, 새 포탑은 오버소팅이 보관.
            _overSorting.Enter(_active, turret);

            RefreshVisual();
        }

        // 파괴 통보 — 포탑이 OnDestroy에서 호출(슬롯이 보유).
        //   가동 파괴:
        //     오버소팅 중이면 → 탄배출 끝. 대기 승격 + 오버소팅이 보관한 pending을 대기로 등록.
        //     일반이면        → 대기를 가동으로 승격, 대기 비움.
        //   대기 파괴 → 대기만 비움. 그 외(이미 빠진 포탑) → no-op.
        public void NotifyTurretDestroyed(ITower turret)
        {
            if (turret == null) return;

            if (turret == _active)
            {
                if (_overSorting.IsActive)
                {
                    // 탄배출 완료 — 최종 동작은 큐가. 대기 승격, 오버소팅에서 pending 받아 대기로.
                    _active = _queue;
                    _queue  = _overSorting.Complete();
                }
                else
                {
                    _active = _queue;   // 대기 승격(없으면 null)
                    _queue  = null;
                }
            }
            else if (turret == _queue)
            {
                _queue = null;
            }
            else
            {
                return;   // 이 슬롯의 가동·대기 아님(이미 빠진 포탑 등) — 무시
            }

            RefreshVisual();
        }

        // 슬롯 파괴(HP 0) 일괄 정리용 — 가동·대기·pending 모두 Despawn + 비움.
        //   오버소팅 중이면 보관 pending까지 정리(락 걸린 채 슬롯 죽으면 데드락 → 강제 해제).
        //   슬롯 Destroyed 상태(SlotRevive 등)가 이걸 호출하도록 연결.
        public void ClearAll()
        {
            DespawnIfTower(_active);
            DespawnIfTower(_queue);

            // 오버소팅 진행 중이면 보관 pending도 회수해 정리(상태·락도 같이 리셋).
            DespawnIfTower(_overSorting.Clear());

            _active = null;
            _queue  = null;
            RefreshVisual();
        }

        // 보유 포탑 실제 삭제 — ITower엔 Despawn seam이 아직 없어 Towers 캐스팅.
        //   포탑 담당이 ITower.Despawn() 추가하면 캐스팅 없이 turret.Despawn()으로 교체.
        //   (Destroy면 OnDestroy→NotifyTurretDestroyed 재호출되나, ClearAll이 먼저 참조 비우므로 no-op로 안전)
        private void DespawnIfTower(ITower turret)
        {
            if (turret == null) return;
            
            if (turret is Towers.Factory.Towers t)
                Destroy(t.gameObject);
        }

        // 큐 변화 → 표시 갱신.
        //   모드(Normal/+Active/+ActiveQueue)·프레임·아이콘·잔탄보드 ON/OFF·총 그림은
        //   컨트롤러가 Refresh→Apply에서 ITurretPresence(HasActive/타입) 보고 재판정.
        //   잔탄 *숫자*는 발사마다 줄어 구조 변화로 안 잡히므로 SlotAmmoBoard가 Update로 폴링(여기 아님).
        private void RefreshVisual()
        {
            _display?.Refresh();
        }
    }
}
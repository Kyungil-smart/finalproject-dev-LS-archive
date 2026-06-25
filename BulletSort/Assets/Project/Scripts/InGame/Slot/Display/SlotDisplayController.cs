using InGame.Slot.Data;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 표시 지휘소 — SlotState(SlotRevive) + 포탑 유무(ITurretPresence)를 종합해
    //   SlotDisplayMode를 판정하고 각 표시 요소를 켜고 끔(기획 2.1).
    // 비주얼 토글을 한 곳에 모음 — 표가 4모드 × 여러 요소로 커져도 조합이 흩어지지 않게.
    // A안 — 파괴 표시는 프레임(본체) OFF·잔해 ON·HP바 OFF·수리게이지 ON 네 토글.
    //   Slot_Visual이 곧 슬롯 프레임이라 본체 끄기 = 프레임 끄기(별도 Frame 오브젝트 없음).
    // 포탑 유무는 주입식 — 미주입 시 Normal/Destroyed만(포탑 세부는 큐 주입 후).
    //   가동 포탑은 프레임 스프라이트를 그 타입(1~6)으로 교체 + 아이콘 SpriteRenderer를 토글.
    //   대기 포탑은 NormalWithActiveQueue에서만 대기 아이콘 ON(다음 미리보기). 포탑 파괴→모드 하락→요소 자동 복구.
    // 작성자: 이성규
    public class SlotDisplayController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SlotRevive _slotRevive;
        [SerializeField] private SlotVisual _slotVisual;
        [SerializeField] private SlotFloor _slotFloor;

        [Header("표시 요소 — 모드별 on/off 대상")]
        [Tooltip("잔해(DestroyedUnderlay) 오브젝트 — Destroyed에서만 ON")]
        [SerializeField] private GameObject _destroyedUnderlay;

        [Tooltip("HP바 루트 — Normal 계열 표시, Destroyed 숨김")]
        [SerializeField] private GameObject _hpBar;

        [Tooltip("수리 게이지 표시 루트 — Destroyed에서만 표시")]
        [SerializeField] private GameObject _repairGauge;

        [Header("포탑 아이콘 — 가동/대기 표시")]
        [Tooltip("가동 포탑 아이콘 — NormalWithActive 이상에서 ON, 타입별 스프라이트")]
        [SerializeField] private SpriteRenderer _activeTurretIcon;

        [Tooltip("대기 포탑 아이콘(다음 미리보기) — NormalWithActiveQueue에서만 ON")]
        [SerializeField] private SpriteRenderer _queueTurretIcon;

        // 포탑 유무 입력 — 포탑 시스템이 주입. null이면 포탑 없음으로 간주.
        private ITurretPresence _turretPresence;

        // 현재 모드 — 재판정 시 동일하면 토글 생략.
        private SlotDisplayMode _mode = SlotDisplayMode.Normal;

        // 파괴 중 플래그 — OnDestroy 진입 후 Refresh를 막음.
        //   슬롯 파괴 시 자식 포탑도 같이 파괴되며 OnDestroy→NotifyTurretDestroyed→Refresh가 들어오는데,
        //   그땐 슬롯 자식 오브젝트가 이미 파괴 중이라 SetActive가 "파괴 중엔 못 켬" 경고를 냄. 갱신 자체를 건너뜀.
        private bool _destroyed;

        public SlotDisplayMode Mode => _mode;

        private void Awake()
        {
            if (_slotRevive == null)
                _slotRevive = GetComponent<SlotRevive>()
                              ?? GetComponentInChildren<SlotRevive>(includeInactive: true);
            if (_slotVisual == null)
                _slotVisual = GetComponent<SlotVisual>()
                              ?? GetComponentInChildren<SlotVisual>(includeInactive: true);
        }

        private void OnEnable()
        {
            if (_slotRevive != null)
                _slotRevive.OnSlotStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (_slotRevive != null)
                _slotRevive.OnSlotStateChanged -= HandleStateChanged;
        }

        private void OnDestroy()
        {
            // 파괴 절차 진입 — 이후 들어오는 Refresh(포탑 OnDestroy 통보 등)는 SetActive 경고를 내므로 막음.
            _destroyed = true;
        }

        private void OnApplicationQuit()
        {
            // 플레이 종료 — 씬 전체 파괴 순서가 보장 안 돼, 포탑 OnDestroy가 컨트롤러보다 먼저 통보하면
            //   _destroyed가 아직 false라 가드를 샘. 종료 시점에 미리 세워 그 경로까지 막음.
            _destroyed = true;
        }

        private void Start()
        {
            Refresh();  // 초기 표시 1회 — Awake 후 상태 확정 시점.
        }

        // 포탑 시스템이 주입 — 가동/대기 포탑 유무 입력 연결.
        // 주입 후 Refresh로 모드 재판정. 미호출이면 포탑 없는 Normal/Destroyed만.
        public void SetTurretPresence(ITurretPresence presence)
        {
            _turretPresence = presence;
            Refresh();
        }

        // 포탑 유무가 바뀌었을 때 포탑 시스템이 호출 — 모드 재판정.
        public void Refresh()
        {
            // 파괴 중이면 갱신 안 함(SetActive 경고 방지).
            //   _destroyed: 자기 OnDestroy가 먼저 불린 경우.
            //   this == null: 포탑 OnDestroy가 먼저 불려 _destroyed가 아직 false여도,
            //                 슬롯(자기)이 이미 파괴 절차면 Unity 가짜 null로 잡힘.
            if (_destroyed || this == null) return;

            var next = ResolveMode();
            _mode = next;
            Apply(next);
        }

        // 상태 전환(SlotRevive) 수신 → 재판정.
        private void HandleStateChanged(SlotState state) => Refresh();

        // SlotState + 포탑 유무 → 표시 모드 판정
        private SlotDisplayMode ResolveMode()
        {
            // 파괴면 포탑 유무 무관 Destroyed.
            if (_slotRevive != null && _slotRevive.State == SlotState.Destroyed)
                return SlotDisplayMode.Destroyed;

            // 포탑 입력 미주입 → 일반 Normal.
            if (_turretPresence == null || !_turretPresence.HasActiveTurret)
                return SlotDisplayMode.Normal;

            return _turretPresence.HasQueueTurret
                ? SlotDisplayMode.NormalWithActiveQueue
                : SlotDisplayMode.NormalWithActive;
        }

        // 모드에 맞춰 표시 요소 일괄 토글(A안 — 파괴는 잔해 ON·나머지 OFF).
        private void Apply(SlotDisplayMode mode)
        {
            bool destroyed = mode == SlotDisplayMode.Destroyed;

            // 슬롯 프레임(본체) — 파괴 시 OFF (잔해가 대신).
            // Slot_Visual이 곧 프레임이라 본체 끄기 = 프레임 끄기.
            _slotVisual?.SetVisible(!destroyed);

            // 바닥 셀칸 — 파괴 시 어두운 바닥으로 교체(끄는 게 아니라 교체, 잔해 위에서도 보임).
            if (destroyed) _slotFloor?.SetDestroyed();
            else _slotFloor?.SetNormal();

            // 잔해 — Destroyed에서만 ON (Monster 아래 DestroyedUnderlay 레이어).
            if (_destroyedUnderlay != null) _destroyedUnderlay.SetActive(destroyed);

            // HP바 — Normal 계열만.
            if (_hpBar != null) _hpBar.SetActive(!destroyed);

            // 수리 게이지 — Destroyed에서만.
            if (_repairGauge != null) _repairGauge.SetActive(destroyed);

            // 프레임 타입·포탑 아이콘 — 파괴가 아닐 때만 가동 포탑 상태를 반영.
            //   파괴면 프레임은 이미 OFF(잔해가 대신)고 아이콘도 꺼야 하므로 따로 처리.
            ApplyTurretVisual(mode, destroyed);
        }

        // 가동/대기 포탑에 따라 프레임 스프라이트·아이콘을 갱신.
        //   프레임 — 가동 있으면 그 타입(1~6) 프레임, 없으면 기본(0). 파괴면 건드리지 않음(본체 OFF 상태).
        //   아이콘 — 모드별 토글 + 타입 스프라이트. 포탑 없거나 파괴면 OFF.
        //   포탑이 파괴돼 모드가 하락하면(NormalWithActive→Normal 등) 여기서 자동으로 OFF·기본 프레임 복구.
        private void ApplyTurretVisual(SlotDisplayMode mode, bool destroyed)
        {
            // 표시 입력 미주입이거나 파괴 — 아이콘 다 끄고 프레임은 기본(파괴면 프레임 자체가 OFF라 무관).
            if (_turretPresence == null || destroyed)
            {
                SetIcon(_activeTurretIcon, 0);
                SetIcon(_queueTurretIcon, 0);
                if (!destroyed) _slotVisual?.SetDefault();
                return;
            }

            // 가동 포탑 — 있으면 그 타입으로 프레임 교체 + 가동 아이콘 ON, 없으면 기본 프레임.
            int activeType = _turretPresence.ActiveTowerType;
            if (activeType > 0)
            {
                _slotVisual?.SetTowerType(activeType);
                SetIcon(_activeTurretIcon, activeType);
            }
            else
            {
                _slotVisual?.SetDefault();
                SetIcon(_activeTurretIcon, 0);
            }

            // 대기 포탑 — NormalWithActiveQueue에서만 대기 아이콘 ON(다음 미리보기).
            int queueType = mode == SlotDisplayMode.NormalWithActiveQueue
                ? _turretPresence.QueueTowerType
                : 0;
            SetIcon(_queueTurretIcon, queueType);
        }

        // 아이콘 1개 갱신 — towerType 0이면 OFF, 1~6이면 ON + 해당 타입 스프라이트.
        //   스프라이트 미등록(null)이면 OFF로 둠(빈 아이콘 표시 방지).
        private static void SetIcon(SpriteRenderer icon, int towerType)
        {
            if (icon == null) return;

            Sprite sprite = towerType > 0 ? SlotQuery.GetTurretIcon(towerType) : null;
            if (sprite != null)
            {
                icon.sprite = sprite;
                icon.enabled = true;
            }
            else
            {
                icon.enabled = false;
            }
        }
    }
}
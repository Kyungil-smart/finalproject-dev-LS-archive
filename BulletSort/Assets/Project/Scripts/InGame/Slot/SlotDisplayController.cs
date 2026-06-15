using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 표시 지휘소 — SlotState(SlotRevive) + 포탑 유무(ITurretPresence)를 종합해
    //   SlotDisplayMode를 판정하고 각 표시 요소를 켜고 끔(기획 2.1).
    // 비주얼 토글을 한 곳에 모음 — 표가 4모드 × 여러 요소로 커져도 조합이 흩어지지 않게.
    // A안 — 파괴 표시는 프레임(본체) OFF·잔해 ON·HP바 OFF·수리게이지 ON 네 토글.
    //   Slot_Visual이 곧 슬롯 프레임이라 본체 끄기 = 프레임 끄기(별도 Frame 오브젝트 없음).
    // 포탑 유무는 주입식 — 미주입 시 Normal/Destroyed만(포탑 세부는 안정연 데이터 후속).
    //   가동 포탑 테두리는 추후 프레임 스프라이트 교체로 처리(별도 오브젝트 토글 아님).
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

        // 포탑 유무 입력 — 포탑 시스템이 주입. null이면 포탑 없음으로 간주.
        private ITurretPresence _turretPresence;

        // 현재 모드 — 재판정 시 동일하면 토글 생략.
        private SlotDisplayMode _mode = SlotDisplayMode.Normal;

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

        private void Start()
        {
            Refresh();  // 초기 표시 1회 — Awake 후 상태 확정 시점.
        }

        // 포탑 시스템(안정연)이 주입 — 가동/대기 포탑 유무 입력 연결.
        // 주입 후 Refresh로 모드 재판정. 미호출이면 포탑 없는 Normal/Destroyed만.
        public void SetTurretPresence(ITurretPresence presence)
        {
            _turretPresence = presence;
            Refresh();
        }

        // 포탑 유무가 바뀌었을 때 포탑 시스템이 호출 — 모드 재판정.
        public void Refresh()
        {
            var next = ResolveMode();
            _mode = next;
            Apply(next);
        }

        // 상태 전환(SlotRevive) 수신 → 재판정.
        private void HandleStateChanged(SlotState state) => Refresh();

        // SlotState + 포탑 유무 → 표시 모드 판정(기획 2.1).
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
        }
    }
}
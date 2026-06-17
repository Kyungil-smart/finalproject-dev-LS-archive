using System;
using InGame.Slot.Data;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 상태 — 파괴/정상. 수리중은 Destroyed + RepairCount로 표현(별도 상태 아님).
    public enum SlotState { Normal, Destroyed }

    // 슬롯 부활·수리 로직 — SlotHealth.OnDead로 파괴 진입, 파괴 중 정렬 성공(Slot.OnRepairProgress)마다
    //   RepairCount 누적, RequiredRepairCount 도달 시 SlotHealth.Revive로 복구.
    // 표시는 직접 안 들고 상태·카운트 이벤트만 발행. 각 표시(컨트롤러·수리카운트)가 자기 구독.
    // SlotHealth(HP)·SlotVisual(표시)와 분리 — 부활 전용 로직만 단독 소유.
    // 작성자: 이성규
    public class SlotRevive : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slot _slot;
        [SerializeField] private SlotHealth _slotHealth;

        // 부활 기획값 — Awake에서 SlotData로부터 주입. 미조회 시 폴백.
        private int _requiredRepairCount = 3;
        private int _reviveHP = 100;

        // 런타임 상태 — 기획서 SlotRuntimeData 대응.
        private SlotState _state = SlotState.Normal;
        private int _repairCount;

        public SlotState State => _state;
        public int RepairCount => _repairCount;
        public int RequiredRepairCount => _requiredRepairCount;

        // 상태 전환 — 표시 컨트롤러·HP바가 구독해 표시 전환(파괴/정상).
        public event Action<SlotState> OnSlotStateChanged;

        // 수리 카운트 변경 — 수리 카운트 표시가 구독. (현재, 필요)
        public event Action<int, int> OnRepairCountChanged;

        private void Awake()
        {
            if (_slot == null)
                _slot = GetComponent<Slot>() ?? GetComponentInParent<Slot>();
            if (_slotHealth == null)
                _slotHealth = GetComponent<SlotHealth>()
                              ?? GetComponentInChildren<SlotHealth>(includeInactive: true);

            // SlotDataID는 슬롯에서 받아옴 — 단일 출처. _slot이 이미 위에서 잡힘.
            int slotDataID = _slot != null ? _slot.SlotDataID : 0;
            
            // 부활 기획값 주입 — SlotData(미조회 시 폴백). ReviveHPValue 0이면 MaxHP까지.
            var data = SlotQuery.Get(slotDataID);
            if (data != null)
            {
                _requiredRepairCount = data.RequiredRepairCount;
                _reviveHP = data.ReviveHPValue > 0 ? data.ReviveHPValue : data.MaxHP;
            }
            else
            {
                Debug.LogWarning($"[SlotRevive] SlotData({slotDataID}) 미조회 — 폴백값 사용");
            }
        }

        private void OnEnable()
        {
            if (_slotHealth != null)
                _slotHealth.OnDead += HandleDead;
            if (_slot != null)
                _slot.OnRepairProgress += HandleRepairProgress;
        }

        private void OnDisable()
        {
            if (_slotHealth != null)
                _slotHealth.OnDead -= HandleDead;
            if (_slot != null)
                _slot.OnRepairProgress -= HandleRepairProgress;
        }

        // 파괴 진입 — 상태 전환 + 수리 카운트 초기화. 표시는 구독자가 갱신.
        private void HandleDead(SlotHealth health)
        {
            if (_state == SlotState.Destroyed) return;

            _state = SlotState.Destroyed;
            _repairCount = 0;

            OnSlotStateChanged?.Invoke(_state);
            OnRepairCountChanged?.Invoke(_repairCount, _requiredRepairCount);
        }

        // 파괴 중 정렬 성공 — 수리 카운트 누적. 도달 시 부활.
        private void HandleRepairProgress()
        {
            if (_state != SlotState.Destroyed) return;

            _repairCount++;
            OnRepairCountChanged?.Invoke(_repairCount, _requiredRepairCount);

            if (_repairCount >= _requiredRepairCount)
                Revive();
        }

        // 부활 — HP 복구 + 상태 정상 전환. 표시는 구독자가 갱신.
        private void Revive()
        {
            _slotHealth?.Revive(_reviveHP);

            _state = SlotState.Normal;
            _repairCount = 0;

            OnSlotStateChanged?.Invoke(_state);
        }
    }
}
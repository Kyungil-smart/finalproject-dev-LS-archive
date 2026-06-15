using System;
using TMPro;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 수리 진행 표시 — SlotRevive.OnRepairCountChanged 구독해 "n/3" 텍스트 갱신.
    // 데이터→비주얼 단방향: SlotRevive(수리 카운트) → 이 컴포넌트(TMP 텍스트). SlotHealthBar와 같은 결.
    // 표시 ON/OFF(파괴 중에만)는 SlotDisplayController가 _repairGauge 토글로 처리 — 여긴 텍스트 갱신만.
    // 작성자: 이성규
    public class SlotRepairGauge : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("수리 카운트 텍스트 — 비우면 Awake에서 탐색")]
        [SerializeField] private TMP_Text _countText;

        [Tooltip("구독할 부활 컴포넌트 — 비우면 Awake에서 탐색")]
        [SerializeField] private SlotRevive _slotRevive;

        private void Awake()
        {
            if(_countText == null)
                _countText = GetComponentInChildren<TMP_Text>();
            if(_slotRevive == null)
                _slotRevive = GetComponentInParent<SlotRevive>();
        }

        private void OnEnable()
        {
            if (_slotRevive != null)
            {
                _slotRevive.OnRepairCountChanged += HandleRepairCountChanged;
                // 켜진 시점의 현재값 1회 반영 — 이벤트 발행이 SetActive보다 앞서 놓친 초기값 보정.
                HandleRepairCountChanged(_slotRevive.RepairCount, _slotRevive.RequiredRepairCount);
            }
        }

        private void OnDisable()
        {
            if (_slotRevive != null)
                _slotRevive.OnRepairCountChanged -= HandleRepairCountChanged;
        }
        
        // 수리 카운트 변경 수신 → "현재/필요" 텍스트 갱신.
        private void HandleRepairCountChanged(int count, int required)
        {
            if (_countText != null)
                _countText.text = $"{count}/{required}";
        }
    }
}

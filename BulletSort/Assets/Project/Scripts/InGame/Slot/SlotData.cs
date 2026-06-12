using UnityEngine;

namespace InGame.Slot.Data
{
    // 슬롯 1종의 정적 데이터. PieceData와 대칭 — DataManager 테이블에 실려 SlotQuery로 조회.
    // 런타임 변동값(HP·SlotState·RepairCount·CellData)은 SlotRuntimeData/SlotHealth가 갖고,
    // 여기는 변하지 않는 기획값(최대 체력·정렬 회복량·부활 조건)만 보유.
    // 1차 기준 전 슬롯 공통 1개 — SlotID별 차등은 추후 확장(그때 이 SO를 ID별로 늘림).
    // 작성자: 이성규
    [CreateAssetMenu(fileName = "SlotData", menuName = "Scriptable Objects/Temp/SlotData")]
    public class SlotData : ScriptableObject
    {
        [Tooltip("슬롯 데이터 식별자 — DataManager 테이블 키. 데이터 담당 협의 후 확정(임시값)")]
        [SerializeField] private int _slotID;
        
        [Tooltip("슬롯 최대 체력(MaxHP) — 회복해도 이 값을 못 넘음")]
        [SerializeField] private int _maxHP = 100;

        [Tooltip("정렬 성공 시 회복되는 체력(HealOnSortValue)")]
        [SerializeField] private int _healOnSortValue = 10;
 
        [Tooltip("파괴 슬롯 부활에 필요한 정렬 성공 횟수(RequiredRepairCount) — 1차 기준 3 고정")]
        [SerializeField] private int _requiredRepairCount = 3;
        
        [Tooltip("부활 시 회복 체력(ReviveHPValue) — 1차 기준 MaxHP까지")]
        [SerializeField] private int _reviveHPValue;
        
        // 외부 접근용 프로퍼티 (읽기 전용)
        public int SlotID => _slotID;
        public int MaxHP => _maxHP;
        public int HealOnSortValue => _healOnSortValue;
        public int RequiredRepairCount => _requiredRepairCount;
        public int ReviveHPValue => _reviveHPValue;
        
        // ── 후순위 자리 ──
        // ReviveDamage(부활 고정 데미지·넉백) — 기획서 후순위. 부활 로직 들어갈 때 추가.
    }
}

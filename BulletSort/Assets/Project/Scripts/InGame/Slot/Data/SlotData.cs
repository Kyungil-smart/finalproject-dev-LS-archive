using UnityEngine;

namespace InGame.Slot.Data
{
    // 슬롯 1종의 정적 데이터. PieceData와 대칭 — DataManager 테이블에 실려 SlotQuery로 조회.
    // 기획값(체력·회복·부활)과 표시 이미지를 보유. 포탑은 로직만 들고, 슬롯이 가동 포탑 타입별로 이미지를 바꾼다.
    // 작성자: 이성규
    [CreateAssetMenu(fileName = "SlotData", menuName = "Scriptable Objects/Temp/SlotData")]
    public class SlotData : ScriptableObject
    {
        [Tooltip("슬롯 데이터 식별자 — DataManager 테이블 키. 데이터 담당 협의 후 확정(임시값)")]
        [SerializeField] private int _slotDataID;

        [Tooltip("슬롯 최대 체력(MaxHP) — 회복해도 이 값을 못 넘음")]
        [SerializeField] private int _maxHP = 100;

        [Tooltip("정렬 성공 시 회복되는 체력(HealOnSortValue)")]
        [SerializeField] private int _healOnSortValue;

        [Tooltip("파괴 슬롯 부활에 필요한 정렬 성공 횟수(RequiredRepairCount) — 1차 기준 3 고정")]
        [SerializeField] private int _requiredRepairCount = 3;

        [Tooltip("부활 시 회복 체력(ReviveHPValue) — 1차 기준 MaxHP까지")]
        [SerializeField] private int _reviveHPValue;

        // 슬롯 표시 이미지 — 인덱스 = 표시 상태/타입
        //   0   : 기본 이미지(포탑 없음)
        //   1~6 : 가동 포탑 타입(TowerType: AR/Shotgun/Lange/Tank/Wide/Buffer)
        //   7   : 파괴 이미지(Destroyed)
        [Tooltip("슬롯 표시 이미지. 0=기본, 1~6=가동 포탑 타입(TowerType), 7=파괴")]
        [SerializeField] private Sprite[] _slotSprites;

        // 외부 접근용 프로퍼티 (읽기 전용)
        public int SlotDataID => _slotDataID;
        public int MaxHP => _maxHP;
        public int HealOnSortValue => _healOnSortValue;
        public int RequiredRepairCount => _requiredRepairCount;
        public int ReviveHPValue => _reviveHPValue;

        // 기본 슬롯 이미지(포탑 없음)
        public Sprite DefaultSprite => GetSprite(0);

        // 파괴 상태 이미지(Destroyed)
        public Sprite DestroyedSprite => GetSprite(7);

        // 가동 포탑 타입(TowerType 1~6)에 해당하는 슬롯 이미지. 호출부가 타입을 구해 넘김.
        public Sprite GetTowerTypeSprite(int towerType) => GetSprite(towerType);

        // 인덱스로 슬롯 이미지 조회. 범위 밖이면 null.
        private Sprite GetSprite(int index)
        {
            if (_slotSprites == null || index < 0 || index >= _slotSprites.Length) return null;
            return _slotSprites[index];
        }

        // ── 후순위 자리 ──
        // ReviveDamage(부활 고정 데미지·넉백) — 기획서 후순위. 부활 로직 들어갈 때 추가.
    }
}
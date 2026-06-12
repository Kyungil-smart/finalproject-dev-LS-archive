using System;
using Core.Interface.IDamageable;
using InGame.Slot.Data;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 체력 — 전투 피격 처리. IDamageable 구현
    // 슬롯 루트에 부착 (피격 = 슬롯 단위). 루트 콜라이더로 전투 시스템이 IDamageable 접근
    // 루트 스케일(SlotBoardLayout이 해상도 따라 조정)을 콜라이더가 함께 받아 피격 영역도 같이 스케일됨.
    // HP 데이터·계산은 여기서 단독 소유, 외부는 TakeDamage로만 깎음 (캡슐화).
    // Slot(게임 로직)과 분리 — Slot은 정렬·보충만, 피격은 SlotHealth가.
    // 작성자: 이성규
    public class SlotHealth : MonoBehaviour, IDamageable
    {
        // 데이터 미로드 시 폴백 — 정상 경로는 SlotData(SO)에서 MaxHP를 받음.
        private const int FallbackMaxHealth = 100;

        [Tooltip("이 슬롯이 참조할 SlotData ID — 보드 배치 시 부여. 공통 const로 고정하지 않음")]
        [SerializeField] private int _slotDataID;

        // 최대 체력 — Awake에서 SlotData로부터 주입. 데이터 출처는 DataManager로 일원화.
        private int _maxHealth = FallbackMaxHealth;
        
        // 현재 체력 — 외부는 읽기만, 변경은 TakeDamage로만.
        private int _health;
        
        // IDamageable — 외부는 읽기 전용
        public int Health => _health;
        public int MaxHealth => _maxHealth;

        // 파괴 여부 — 외부(스포너 등)가 죽은 슬롯을 타겟 후보에서 거를 때 사용
        public bool isDead => _health <= 0;
        
        // 체력 변경 이벤트 — (현재, 최대). HP 바가 구독해 갱신 (데이터→비주얼 단방향)
        public event Action<int, int> OnHealthChanged;
        
        // 파괴 이벤트 — HP 0 도달 시 발행. Slot·외부가 구독해 후속 처리 (연출·게임오버 등)
        public event Action<SlotHealth> OnDead;

        private void Awake()
        {
            // SlotData(SO)에서 최대 체력 주입 — 슬롯별 SlotDataID로 조회. 미조회 시 폴백.
            var data = SlotQuery.Get(_slotDataID);
            if (data != null)
                _maxHealth = data.MaxHP;
            else
                Debug.LogWarning($"[SlotHealth] SlotData({_slotDataID}) 미조회 — 폴백 MaxHealth 사용");

            _health = _maxHealth;
        }
        
        // 외부(전투 시스템)가 데미지를 넣는 유일한 진입점.
        // HP 계산·클램프·파괴 판정을 내부에서 처리 — 외부는 amount만 넘김
        public void TakeDamage(int amount)
        {
            if(amount <= 0||_health <= 0) return; // 음수·이미 파괴 상태 무시
            
            _health = Mathf.Max(0, _health - amount);
            OnHealthChanged?.Invoke(_health, _maxHealth);

            if (_health == 0)
                Dead();
        }

        // HP 0 도달 시 내부 호출
        private void Dead()
        {
            OnDead?.Invoke(this);
            // TODO(데모 후) — 슬롯 비활성·파괴 연출·게임오버 판정 연결
        }
    }
}
using System;
using Core.Interface.IDamageable;
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
        [Tooltip("슬롯 최대 체력 — 데모 임시값, CBT에서 SlotData SO로 이관")]
        [SerializeField] private int _maxHealth = 100;
        
        // 현재 체력 — 외부는 읽기만, 변경은 TakeDamage로만.
        private int _health;
        
        // IDamageable — 외부는 읽기 전용
        public int Health => _health;
        public int MaxHealth => _maxHealth;
        
        // 체력 변경 이벤트 — (현재, 최대). HP 바가 구독해 갱신 (데이터→비주얼 단방향)
        public event Action<int, int> OnHealthChanged;
        
        // 파괴 이벤트 — HP 0 도달 시 발행. Slot·외부가 구독해 후속 처리 (연출·게임오버 등)
        public event Action<SlotHealth> OnDead;

        public bool isDead = false;

        private void Awake()
        {
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
            Debug.Log("[SlotHealth] Dead");
            isDead = true;
            OnDead?.Invoke(this);
            DeadEvent();
        }

        private void DeadEvent()
        {
            Debug.Log("[SlotHealth] DeadEvent");
            Invoke("TempActive", 0.1f);
            // TODO(데모 후) — 슬롯 비주얼·파괴 연출·게임오버 판정 연결
        }

        // 임시
        private void TempActive()
        {
            gameObject.SetActive(false);
        }
    }
}

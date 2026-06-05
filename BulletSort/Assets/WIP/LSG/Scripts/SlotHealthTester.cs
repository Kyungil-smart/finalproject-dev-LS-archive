using UnityEngine;
using UnityEngine.InputSystem;
using Logger = Core.Logger;

namespace InGame.Slot
{
    // 슬롯 HP 테스트용 — 전투 시스템 없이 피격·HP바·파괴를 검증.
    // 키 입력으로 SlotHealth.TakeDamage 호출. 검증 후 제거 (데모·빌드 미포함 권장).
    // 입력은 New Input System(Keyboard.current) 직접 읽기 — 액션 에셋 불필요.
    // 작성자: 이성규
#if UNITY_EDITOR
    public class SlotHealthTester : MonoBehaviour
    {
        [Tooltip("테스트할 체력 컴포넌트 — 비우면 같은 오브젝트/부모에서 탐색")]
        [SerializeField] private SlotHealth _slotHealth;
        
        [Tooltip("키 한 번에 줄 데미지")]
        [SerializeField] private int _damagePerHit = 10;
        
        [Tooltip("데미지 입력 키")]
        [SerializeField] private Key _damageKey = Key.Space;
        
        private void Awake()
        {
            if (_slotHealth == null)
                _slotHealth = GetComponent<SlotHealth>()
                              ?? GetComponentInParent<SlotHealth>();
        }
        
        private void OnEnable()
        {
            if (_slotHealth != null)
            {
                _slotHealth.OnHealthChanged += LogHealth;
                _slotHealth.OnDead += LogDead;
            }
        }
        
        private void OnDisable()
        {
            if (_slotHealth != null)
            {
                _slotHealth.OnHealthChanged -= LogHealth;
                _slotHealth.OnDead -= LogDead;
            }
        }
        
        private void Update()
        {
            if (_slotHealth == null || Keyboard.current == null) return;
            
            // New Input System — 해당 키가 이번 프레임에 눌렸는지.
            if (Keyboard.current[_damageKey].wasPressedThisFrame)
                _slotHealth.TakeDamage(_damagePerHit);
        }
        
        private void LogHealth(int current, int max)
        {
            Logger.Instance?.LogInfo($"[HP테스트] {name}: {current}/{max}");
        }
        
        private void LogDead(SlotHealth h)
        {
            Logger.Instance?.LogInfo($"[HP테스트] {name}: 파괴됨");
        }
    }
#endif
}
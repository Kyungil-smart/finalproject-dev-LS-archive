using UnityEngine;
using UnityEngine.UI;

namespace InGame.Slot
{
    // 슬롯 HP 바 — SlotHealth의 체력 변경을 구독해 Fill 이미지로 표시.
    // 슬롯 자식 World Space Canvas에 배치 (Canvas Scaler 없이 슬롯 Transform에 스케일 종속).
    // 데이터→비주얼 단방향 — SlotHealth가 진짜 상태, 이 컴포넌트는 비주얼만 갱신.
    public class SlotHealthBar : MonoBehaviour
    {
        [Tooltip("구독할 체력 컴포넌트")]
        [SerializeField] private SlotHealth _slotHealth;
        
        [Tooltip("Fill 이미지 — Image Type을 Filled로 설정")]
        [SerializeField] private Image _fillImage;
        
        private void Awake()
        {
            // 체력 컴포넌트 자동 탐색 — 인스펙터 미지정 시 캐싱.
            if (_slotHealth == null)
                _slotHealth = GetComponent<SlotHealth>()
                              ?? GetComponentInChildren<SlotHealth>();
        }

        private void OnEnable()
        {
            if (_slotHealth == null) return;
            
            _slotHealth.OnHealthChanged += HandleHealthChanged;
        }
        
        // 초기 표시는 Start에서 — 모든 Awake 후라 SlotHealth._health가 확실히 채워진 시점.
        // (OnEnable은 Awake 순서가 보장 안 돼 _health가 0인 채로 읽힐 수 있음)
        private void Start()
        {
            if (_slotHealth == null) return;
            
            // 구독 직후 현재값으로 한 번 맞춤 — 초기 체력 표시.
            HandleHealthChanged(_slotHealth.Health, _slotHealth.MaxHealth);
        }

        private void OnDisable()
        {
            if (_slotHealth != null)
                _slotHealth.OnHealthChanged -= HandleHealthChanged;
        }
        
        // 체력 변경 → Fill 갱신. (현재 / 최대) 비율을 fillAmount(0~1)로
        private void HandleHealthChanged(int current, int max)
        {
            if (_fillImage == null) return;
            
            float ratio = max > 0 ? ((float)current / max) : 0;
            _fillImage.fillAmount = ratio;
        }
    }
}
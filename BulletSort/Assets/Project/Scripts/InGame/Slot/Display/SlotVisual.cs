using InGame.Slot.Data;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 표시 비주얼 — SlotData의 타입별 스프라이트를 본체 SpriteRenderer에 반영.
    // 슬롯이 자기 이미지를 들고, 포탑은 순수 로직. 가동 포탑 타입에 따라 본체 이미지를 교체.
    // 데이터→비주얼 단방향: SlotData(정적 이미지 출처) → 이 컴포넌트(렌더러 갱신).
    // 파괴 표시(본체 OFF/잔해 ON/Frame OFF)는 SlotDisplayController가 오브젝트 토글로 처리 — 여긴 본체 이미지·표시만.
    // 작성자: 이성규
    public class SlotVisual : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("슬롯 본체 렌더러 — 비우면 Awake에서 탐색")]
        [SerializeField] private SpriteRenderer _renderer;

        // SlotData 캐싱 — Awake 1회 조회. 미조회 시 null, 각 Set은 가드로 무시.
        private SlotData _data;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>()
                            ?? GetComponentInChildren<SpriteRenderer>();

            // SlotDataID는 슬롯에서 받아옴 — 단일 출처.
            var slot = GetComponent<Slot>()
                       ?? GetComponentInParent<Slot>();
            int slotDataID = slot != null ? slot.SlotDataID : 0;
            
            _data = SlotQuery.Get(slotDataID);
            if (_data == null)
                Debug.LogWarning($"[SlotVisual] SlotData({slotDataID}) 미조회 — 표시 갱신 무시됨");
        }

        // 초기 표시 — 기본(포탑 없음)으로 한 번 맞춤.
        private void Start()
        {
            SetDefault();
        }

        // 기본 표시(포탑 없음) — 인덱스 0.
        public void SetDefault()
        {
            Apply(_data?.DefaultSprite);
        }

        // 가동 포탑 타입(1~6)으로 표시 교체. 타입 변환은 호출부 책임 — 여기는 int만 받음.
        public void SetTowerType(int towerType)
        {
            Apply(_data?.GetTowerTypeSprite(towerType));
        }

        // 본체 표시 ON/OFF — 파괴 시 끔(잔해가 대신함). 렌더러만 끔(컴포넌트는 유지).
        public void SetVisible(bool visible)
        {
            if (_renderer != null)
                _renderer.enabled = visible;
        }

        // 본체 렌더러에 스프라이트 반영 — null이면 무시(빈 칸 깜빡임 방지).
        private void Apply(Sprite sprite)
        {
            if (_renderer == null || sprite == null) return;
            _renderer.sprite = sprite;
        }
    }
}
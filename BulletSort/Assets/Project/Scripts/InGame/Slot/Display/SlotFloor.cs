using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 바닥(셀칸) 표시 — Slot_Bottom_White 4개(Back/Left/Center/Right)의 스프라이트를 상태에 따라 교체.
    // 파괴 시 어두운 바닥으로, 정상/부활 시 원래 바닥으로. SlotVisual(프레임)과 분리 —
    //   프레임은 파괴 시 꺼지지만(잔해가 대신), 바닥은 잔해 위에서도 보여 *교체*가 맞음.
    // 정상 스프라이트는 Awake에서 프리팹 현재값을 캐싱 — 인스펙터엔 파괴용만 지정.
    // 표시 전환은 SlotDisplayController가 지휘 — 여긴 시키는 대로 스프라이트만 교체.
    // 작성자: 이성규
    public class SlotFloor : MonoBehaviour
    {
        [Header("References")] [Tooltip("바닥 셀칸 렌더러 4개 — Back/Left/Center/Right 순서 무관, 인스펙터로 직접 지정")] [SerializeField]
        private SpriteRenderer[] _floorRenderers;

        [Tooltip("파괴 바닥 스프라이트(어두운) — 렌더러와 같은 순서로 대응")] [SerializeField]
        private Sprite[] _destroyedSprites;
        
        // 정상 스프라이트 캐싱 — 프리팹에 이미 꽂힌 현재 스프라이트가 정상값.
        private Sprite[] _normalSprites;
        
        private void Awake()
        {
            if (_floorRenderers == null) return;

            _normalSprites = new Sprite[_floorRenderers.Length];
            for (int i = 0; i < _floorRenderers.Length; i++)
                _normalSprites[i] = _floorRenderers[i] != null ? _floorRenderers[i].sprite : null;
        }

        // 정상 바닥 표시.
        public void SetNormal()
        {
            ApplySprite(_normalSprites);
        }

        // 파괴 바닥(어두운) 표시.
        public void SetDestroyed()
        {
            ApplySprite(_destroyedSprites);
        }

        // 렌더러 배열에 스트라이트 배열을 인덱스 대응으로 반영.
        private void ApplySprite(Sprite[] sprites)
        {
            if (_floorRenderers == null || sprites == null) return;
            
            int count = Mathf.Min(_floorRenderers.Length, sprites.Length);
            for (int i = 0; i < count; i++)
            {
                if (_floorRenderers[i] != null && sprites[i] != null)
                    _floorRenderers[i].sprite = sprites[i];
            }
        }
    }
}
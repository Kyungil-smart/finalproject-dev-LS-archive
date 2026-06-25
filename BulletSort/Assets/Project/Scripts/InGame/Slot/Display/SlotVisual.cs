using InGame.Slot.Data;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 본체(프레임) 표시 — 가동 포탑 타입에 따라 프레임 스프라이트를 SpriteRenderer에 반영.
    // 프레임 자산은 SlotTurretSpriteTable로 분리, SlotQuery로 조회(데이터→비주얼 단방향).
    //   인덱스: 0=기본(포탑 없음), 1~6=가동 포탑 타입(TowerType), 7=파괴.
    // 파괴 표시(본체 OFF/잔해 ON)는 SlotDisplayController가 오브젝트 토글로 처리 — 여긴 프레임 이미지·표시만.
    // 작성자: 이성규
    public class SlotVisual : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("슬롯 본체 렌더러 — 비우면 Awake에서 탐색")]
        [SerializeField] private SpriteRenderer _renderer;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>()
                            ?? GetComponentInChildren<SpriteRenderer>();
        }

        // 초기 표시 — 기본(포탑 없음)으로 한 번 맞춤.
        private void Start()
        {
            SetDefault();
        }

        // 기본 표시(포탑 없음) — 인덱스 0.
        public void SetDefault()
        {
            Apply(SlotQuery.GetFrame(0));
        }

        // 가동 포탑 타입(1~6)으로 표시 교체. 타입 변환은 호출부 책임 — 여기는 int만 받음.
        public void SetTowerType(int towerType)
        {
            Apply(SlotQuery.GetFrame(towerType));
        }

        // 본체 표시 ON/OFF — 파괴 시 끔(잔해가 대신함). 렌더러만 끔(컴포넌트는 유지).
        //   파괴 표시는 컨트롤러가 본체 OFF + Slot_DestroyedUnderlay 오브젝트 ON으로 처리 — 프레임 7번은 안 씀.
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
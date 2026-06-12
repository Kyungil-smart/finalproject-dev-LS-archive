using System;
using InGame.Slot.Data;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 표시 비주얼 — SlotData의 상태/타입별 스프라이트를 SpriteRenderer에 반영.
    // 슬롯이 자기 이미지를 들고, 포탑은 순수 로직. 가동 포탑 타입에 따라 슬롯이 자기 표시를 교체
    // 데이터→비주얼 단방향: SlotData(정적 이미지 출처) → 이 컴포넌트(렌더러 갱신). SlotHealthBar와 같은 결.
    // 교체 시점 책임은 호출부: 정렬 즉시가 아니라 가동 포탑(ActiveTurret) 확정 시점에 호출
    // 작성자: 이성규
    public class SlotVisual : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("슬롯 표시 스프라이트 렌더러 — 비우면 Awake에서 탐색")]
        [SerializeField] private SpriteRenderer _renderer;
        
        [Tooltip("이 슬롯이 참조할 SlotData ID — SlotHealth와 같은 값. 보드 배치 시 부여")]
        [SerializeField] private int _slotDataID;
        
        // SlotData 캐싱 — Awake 1회 조회. 미조회 시 null, 각 Set은 가드로 무시.
        private SlotData _data;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>()
                            ?? GetComponentInChildren<SpriteRenderer>();
            
            // SlotData 캐싱 — SlotHealth와 동일하게 _slotDataID로 조회(데이터 출처 DataManager 일원화).
            _data = SlotQuery.Get(_slotDataID);
            if (_data == null)
                Debug.LogWarning($"[SlotVisual] SlotData({_slotDataID}) 미조회 — 표시 갱신 무시됨");
        }

        // 초기 표시 — 모든 Awake 후 기본(포탑 없음)으로 한 번 맞춤.
        // SlotHealthBar가 Start에서 초기 HP를 그리는 것과 같은 시점(Awake 순서 비의존).
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

        // 파괴 표시(Destroyed 잔해) — 인덱스 7.
        public void SetDestroyed()
        {
            Apply(_data?.DestroyedSprite);
        }
        
        // 렌더러에 스프라이트 반영 — null이면 무시(이전 표시 유지, 빈 칸으로 깜빡임 방지).
        private void Apply(Sprite sprite)
        {
            if (_renderer == null || sprite == null) return;
            _renderer.sprite = sprite;
        }
    }
}

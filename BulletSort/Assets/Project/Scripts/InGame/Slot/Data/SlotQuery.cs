using Core;
using UnityEngine;

namespace InGame.Slot.Data
{
    // 슬롯 정적 데이터·표시 스프라이트 조회.
    //   - SlotData(수치: 체력·회복·부활)는 DataManager 테이블에서 SlotDataID로 조회.
    //   - 표시 스프라이트(프레임·포탑 아이콘·잔탄보드)는 SlotTurretSpriteTable에서 TowerType 키로 조회.
    // PieceQuery와 대칭: 데이터 보유는 DataManager(자동 적재)·SpriteTable(에셋 참조), 도메인 조회·가공은 여기.
    //   슬롯 프레임은 TowerType에 따라서만 바뀌고 슬롯 위치와 무관 → 테이블 하나를 모든 슬롯이 공유.
    //   추후 어드레서블 전환 시 SpriteTable 로드 한 줄만 교체(Resources.Load → Addressables).
    public static class SlotQuery
    {
        // 슬롯 표시 스프라이트 테이블 — 에셋 참조라 DataManager(CSV 자동 적재)와 별개. Query가 직접 로드·캐싱.
        private static SlotTurretSpriteTable _spriteTable;
        private static SlotTurretSpriteTable SpriteTable =>
            _spriteTable != null ? _spriteTable
                : (_spriteTable = Resources.Load<SlotTurretSpriteTable>("SpriteTables/SlotTurretSpriteTable"));
        
        // SlotData ID로 조회. 없으면 null (호출 측에서 폴백 처리).
        public static SlotData Get(int slotDataID)
        {
            return DataManager.Instance.GetData<SlotData>(slotDataID);
        }
        
        // 슬롯 프레임 스프라이트 — 인덱스(0=기본·1~6=TowerType·7=파괴). 미등록이면 null.
        public static Sprite GetFrame(int index)
        {
            return SpriteTable != null ? SpriteTable.GetFrame(index) : null;
        }
        
        // 가동 포탑 아이콘(상단 마크) — TowerType(1~6). 미등록이면 null(호출부가 빈 칸 처리).
        public static Sprite GetTurretIcon(int towerType)
        {
            return SpriteTable != null ? SpriteTable.GetTurretIcon(towerType) : null;
        }
        
        // 잔탄보드 총 그림 — TowerType(1~6). 미등록이면 null.
        public static Sprite GetAmmoBoardIcon(int towerType)
        {
            return SpriteTable != null ? SpriteTable.GetAmmoBoardIcon(towerType) : null;
        }
    }
}
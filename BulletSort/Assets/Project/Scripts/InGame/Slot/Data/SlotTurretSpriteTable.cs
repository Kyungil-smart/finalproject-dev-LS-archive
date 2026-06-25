using UnityEngine;

namespace InGame.Slot.Data
{
    // 슬롯 표시 스프라이트 테이블 — 슬롯 프레임·포탑 아이콘·잔탄보드 총그림을 TowerType 키로 보유.
    //   PieceSpriteTable과 대칭(자산은 SO 테이블, 조회는 Query). 단 키가 이름(string) 아닌 인덱스(int)라 배열로 단순화.
    // 슬롯 프레임은 *TowerType에 따라서만* 바뀌고 슬롯 위치(SlotDataID)와 무관 → 모든 슬롯이 이 테이블 하나를 공유.
    //   (이전엔 SlotData가 슬롯별로 스프라이트를 들었으나, 위치 무관·타입 종속이라 단일 테이블로 통일)
    // 인덱스 체계(프레임) — 기존 SlotData._slotSprites와 동일:
    //   0   : 초기 상태(포탑 없음, 기본 프레임)
    //   1~6 : 가동 포탑 타입(TowerType: AR/Shotgun/Lange/Tank/Wide/Buffer)
    //   7   : 파괴(Destroyed)
    // 자동 생성 대상 아님 — 에셋 참조는 사람이 꽂는 개인 SO.
    // 작성자: 이성규
    [CreateAssetMenu(fileName = "SlotTurretSpriteTable", menuName = "Scriptable Objects/SlotTurretSpriteTable")]
    public class SlotTurretSpriteTable : ScriptableObject
    {
        [Tooltip("슬롯 프레임. 0=기본, 1~6=가동 포탑 타입(TowerType), 7=파괴")]
        [SerializeField] private Sprite[] _frames;

        [Tooltip("가동 포탑 아이콘(상단 마크). 인덱스=TowerType(1~6). 0번 칸은 비움(포탑 없음=아이콘 없음)")]
        [SerializeField] private Sprite[] _turretIcons;

        [Tooltip("잔탄보드 총 그림. 인덱스=TowerType(1~6). 0번 칸은 비움")]
        [SerializeField] private Sprite[] _ammoBoardIcons;
        
        // 슬롯 프레임 — 인덱스(0=기본·1~6=타입·7=파괴)로 조회. 범위 밖이면 null.
        public Sprite GetFrame(int index) => GetAt(_frames, index);
 
        // 가동 포탑 아이콘 — TowerType(1~6)으로 조회. 범위 밖/미등록이면 null(호출부가 빈 칸 처리).
        public Sprite GetTurretIcon(int towerType) => GetAt(_turretIcons, towerType);
        
        // 잔탄보드 총 그림 — TowerType(1~6)으로 조회. 범위 밖/미등록이면 null.
        public Sprite GetAmmoBoardIcon(int towerType) => GetAt(_ammoBoardIcons, towerType);
        
        private static Sprite GetAt(Sprite[] arr, int index)
        {
            if (arr == null || index < 0 || index >= arr.Length) return null;
            return arr[index];
        }
    }
}

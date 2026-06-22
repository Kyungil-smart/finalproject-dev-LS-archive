using UnityEngine;

namespace InGame.Sort.Data
{
    // 기물 1종의 데이터. 임시 SO — 데이터 담당 CSV→partial 자동 생성본 들어오면 교체.
    // 시트 컬럼과 필드명 일치(교체 시 파일 통째로 갈아끼움). 스프라이트는 이름(string)만 들고,
    // 실제 Sprite 객체는 PieceQuery가 PieceSpriteTable에서 이름으로 조회 — 데이터/에셋 분리.
    // 작성자: 이성규
    [CreateAssetMenu(fileName = "PieceData", menuName = "Scriptable Objects/Temp/PieceData")]
    public class PieceData : ScriptableObject
    {
        // ID는 DataManager.GetIdFromSO가 public 필드로 읽음(자동 생성 컨벤션, NonPublic 의존 회피).
        [Tooltip("기물 식별자 — 정렬 판정·포탑 타입 결정 기준. 0은 빈 칸 예약값이라 사용 안 함")]
        public int PieceID;

        [Tooltip("기물 이름 — 표시할 유닛 이름")]
        public string PieceName;

        [Tooltip("기물 타입(PieceType) — 1~6 (AR/Shotgun/Lange/Tank/Wide/Buffer)")]
        public int PieceType;

        [Tooltip("기물 성급 — 다른 성급이면 같은 기물도 별도 선택 가능")]
        public int PieceGrade;

        [Tooltip("기물 레벨 — 레벨별 다른 ID 보유, 레벨업 시 ID 교체 방식")]
        public int PieceLv;

        [Tooltip("연결 포탑 ID — 정렬 성공 시 소환할 포탑(ERD ConnectTower). 타워가 이 ID로 자기 타입 조회")]
        public int ConnectTower;

        [Tooltip("인게임 기물 스프라이트 '이름' — PieceQuery가 PieceSpriteTable에서 이 이름으로 조회")]
        public string PieceSprite;

        [Tooltip("덱 카드용 초상화 '이름' — 조회·표시는 F(덱 카드) 작업 때. 지금은 필드만")]
        public string Portrait;
    }
}
using UnityEngine;

namespace InGame.Sort.Data
{
    // 기물 1종의 데이터. 임시 SO — 데이터 담당의 정식 PieceData SO/파싱 결과가 들어오면 교체
    // 지금은 스프라이트 연결만 담아 "두 번 작업" 방지: Piece가 enum·배열 대신 이 SO를 조회하는 구조
    // 정식 SO 도입 시 이 클래스의 필드만 늘리거나 출처만 갈아 끼우면 됨
    // (등급·연결 포탑·스탯 등은 ERD 확정 후 추가될 자리 — 지금은 비워둠)
    // 작성자: 이성규
    [CreateAssetMenu(fileName = "PieceData", menuName = "Scriptable Objects/Temp/PieceData")]
    public class PieceData : ScriptableObject
    {
        // ID는 DataManager.GetIdFromSO가 public 필드로 읽음(자동 생성 SO 컨벤션과 동일).
        // private+프로퍼티는 NonPublic 리플렉션 의존이라, 자동 생성이 DataManager를 덮으면 깨짐 → public 필드로 통일.
        [Tooltip("기물 식별자 — 정렬 판정·포탑 타입 결정의 기준. 0은 빈 칸 예약값이라 사용 안 함")]
        public int PieceID;

        [Tooltip("기물 스프라이트 — SetByID 시 이 스프라이트로 교체")]
        [SerializeField] private Sprite _sprite;
        
        [Tooltip("연결 포탑 ID — 정렬 성공 시 소환할 포탑 식별자(ERD의 ConnectTower). 타워 영역이 이 ID로 자기 타입 조회")]
        [SerializeField] private int _connectTowerID;

        // 외부 접근용 프로퍼티
        public Sprite Sprite => _sprite;
        public int ConnectTowerID => _connectTowerID;
        
        // ── 정식 SO 도입 시 추가될 자리 ──
        // 등급(PieceGrade), 레벨(PieceLv) 등.
        // ConnectTowerID는 ERD의 기물↔포탑 FK — 타워 enum 대신 int로 들어 결합 회피.
        // 지금은 스프라이트 + 연결 포탑 ID까지.
    }
}

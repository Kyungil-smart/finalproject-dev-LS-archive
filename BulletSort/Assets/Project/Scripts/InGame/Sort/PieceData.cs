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
        [Tooltip("기물 식별자 — 정렬 판정·포탑 타입 결정의 기준. 0은 빈 칸 예약값이라 사용 안 함")]
        [SerializeField] private int _pieceID;

        [Tooltip("기물 종류 — 인스펙터 가독성용. ID와 의미가 일치하도록 함께 지정")]
        [SerializeField] private PieceType _pieceType;

        [Tooltip("기물 스프라이트 — SetByID 시 이 스프라이트로 교체")]
        [SerializeField] private Sprite _sprite;

        // 외부 접근용 프로퍼티
        public int PieceID => _pieceID;
        public PieceType PieceType => _pieceType;
        public Sprite Sprite => _sprite;
        
        // ── 정식 SO 도입 시 추가될 자리 ──
        // 등급(PieceGrade), 레벨(PieceLv), 연결 포탑(ConnectTower) 등.
        // 지금은 스프라이트 연결만 — 비주얼 교체가 이번 작업 범위.
    }
}

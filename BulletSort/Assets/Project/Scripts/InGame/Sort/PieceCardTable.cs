using UnityEngine;

namespace InGame.Sort.Data
{
    // 덱 카드 비주얼 테이블 — 카드 프레임·배경을 PieceType(1~6) 키로 보유.
    //   SlotTurretSpriteTable과 같은 결(키가 이름 아닌 인덱스라 배열로 단순화).
    // 프레임·배경은 *PieceType에 따라서만* 바뀜(타입별로 동일) → 기물마다 이름 안 들고 타입으로 직접 조회.
    //   (초상화는 캐릭터별이라 이름 기반(PieceSpriteTable), 프레임·배경은 타입별이라 인덱스 기반으로 분리)
    // 인덱스 = PieceType(1~6: AR/Shotgun/Lange/Tank/Wide/Buffer). 0번 칸은 비움(타입 0 = 빈 칸 예약).
    // 자동 생성 대상 아님 — 에셋 참조는 사람이 꽂는 개인 SO.
    //   (기획팀 이름 테이블 도착 전 임시 — 타입 기반이라 이름 데이터 없이도 동작. 이름 와도 프레임·배경은 무관)
    // 작성자: 이성규
    [CreateAssetMenu(fileName = "PieceCardTable", menuName = "Scriptable Objects/PieceCardTable")]
    public class PieceCardTable : ScriptableObject
    {
        [Tooltip("카드 프레임. 인덱스=PieceType(1~6). 0번 칸은 비움")]
        [SerializeField] private Sprite[] _frames;

        [Tooltip("카드 배경. 인덱스=PieceType(1~6). 0번 칸은 비움")]
        [SerializeField] private Sprite[] _backgrounds;

        // 카드 프레임 — PieceType(1~6)으로 조회. 범위 밖/미등록이면 null(호출부가 빈 칸 처리).
        public Sprite GetFrame(int pieceType) => GetAt(_frames, pieceType);

        // 카드 배경 — PieceType(1~6)으로 조회. 범위 밖/미등록이면 null.
        public Sprite GetBackground(int pieceType) => GetAt(_backgrounds, pieceType);

        private static Sprite GetAt(Sprite[] arr, int index)
        {
            if (arr == null || index < 0 || index >= arr.Length) return null;
            return arr[index];
        }
    }
}
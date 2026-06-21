using System.Collections.Generic;

namespace Lobby.Deck
{
    // 덱 전달소 — 로비(덱 편성)에서 인게임으로 넘어갈 때 편성 덱 ID를 보관.
    // 씬이 갈려 인스턴스 참조가 끊기므로 static으로 다리만 놓음.
    // SlotBoardManager가 인게임 진입 시 Consume으로 읽어 대기 그룹 구성.
    //   비어 있으면(덱 안 거치고 바로 인게임 등) 호출부가 폴백(GetAllIDs).
    // 정식: 세이브/덱 데이터 시스템(김경민) 들어오면 이 static 다리를 그 경유로 교체.
    // 작성자: 이성규
    public static class DeckHolder
    {
        private static List<int> _deckPieceIDs;

        // 편성된 덱이 있나 (6칸 중 하나라도 채워졌나)
        public static bool HasDeck => _deckPieceIDs != null && _deckPieceIDs.Count > 0;

        // 로비에서 편성 완료 시 저장 (시작 버튼)
        public static void Set(IReadOnlyList<int> deckPieceIDs)
        {
            _deckPieceIDs = new List<int>(deckPieceIDs);
        }

        // 인게임에서 읽기 — 복사본 반환(원본 보호). 없으면 null → 호출부 폴백.
        public static IReadOnlyList<int> Get()
        {
            return _deckPieceIDs != null ? new List<int>(_deckPieceIDs) : null;
        }

        // 인게임 진입 후 비우기 — 다음 진입에 이전 덱이 남지 않게(원하면 호출).
        public static void Clear()
        {
            _deckPieceIDs = null;
        }
    }
}
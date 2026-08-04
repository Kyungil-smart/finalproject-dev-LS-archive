using System.Collections.Generic;

namespace Lobby.Deck
{
    // 보유 목록 필터 — 정렬 팝업이 만들고 DeckBuilder가 적용.
    //   Owned : 전체/보유/미보유 중 1택.
    //   Types : 켜진 유형(PieceType 1~6)만 표시. 비어 있으면 전체 유형.
    // 작성자: 이성규
    public enum OwnedFilter { All, Owned, NotOwned }

    public class SortFilter
    {
        public OwnedFilter Owned = OwnedFilter.All;

        // 비어 있으면 전체 유형 — '전부 켬'과 '전부 끔'이 같은 결과(전체 표시).
        public readonly HashSet<int> Types = new HashSet<int>();

        // 이 카드가 필터를 통과하는지. (보유 여부·유형은 호출부가 구해서 넘김)
        public bool Pass(bool isOwned, int type)
        {
            if (Owned == OwnedFilter.Owned && !isOwned) return false;
            if (Owned == OwnedFilter.NotOwned && isOwned) return false;

            if (Types.Count > 0 && !Types.Contains(type)) return false;

            return true;
        }

        // 다른 필터의 내용을 이쪽으로 복사 — 인스턴스는 유지(참조 공유 방지).
        public void CopyFrom(SortFilter other)
        {
            if (other == null) return;

            Owned = other.Owned;
            Types.Clear();
            foreach (var t in other.Types) Types.Add(t);
        }
    }
}
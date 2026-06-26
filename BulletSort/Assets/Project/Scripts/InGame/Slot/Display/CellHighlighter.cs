using InGame.Slot;
using UnityEngine;

namespace InGame.Sort
{
    // 셀 하이라이트 — 드래그 중인 기물이 호버한 셀을 강조.
    //   "이 기물이 어느 칸에 들어갈지"를 드롭 전에 보여줌. 항상 한 칸만 표시.
    // Piece에 붙어 Piece의 드래그 콜백에서 위임받음 — UpdateHover(드래그 중)/Clear(놓기·잡기).
    //   셀 찾기(Cell 레이어 raycast)는 Piece가 이미 쓰는 _cellLayer를 Init으로 넘겨받아 공유.
    // 하이라이트 조건 — 호버 셀이 *빈 칸* AND 슬롯이 *락 아님*(오버소팅 중 배치 불가).
    //   파괴 슬롯은 통과(정렬로 수리되므로 배치 유효). 색만 바꿔 DestroyedUnderlay와 겹쳐도 무난.
    // 작성자: 이성규
    public class CellHighlighter : MonoBehaviour
    {
        // 셀 판정 레이어 — Piece에서 Init으로 넘겨받음(같은 _cellLayer 공유, 중복 지정 방지).
        private LayerMask _cellLayer;

        // 현재 하이라이트 중인 셀 — 한 칸만 유지. 호버 이동 시 직전 셀 끄고 새 셀 켬.
        private SlotCell _current;

        // Piece가 드래그 시작 전 1회 호출 — 셀 레이어 주입.
        public void Init(LayerMask cellLayer)
        {
            _cellLayer = cellLayer;
        }

        // 드래그 중 매 프레임 — 호버 셀 판정 후 하이라이트 갱신(Piece.OnDragging에서 호출).
        public void UpdateHover(Vector2 worldPos)
        {
            SlotCell cell = FindCellAt(worldPos);

            // 배치 가능 판정 — 빈 칸 AND 슬롯 락 아님. 아니면 하이라이트 대상 아님(null 취급).
            if (cell != null && !CanHighlight(cell))
                cell = null;

            if (cell == _current) return;   // 같은 셀이면 갱신 불필요

            // 직전 셀 끄고 새 셀 켜기 — 항상 한 칸만.
            if (_current != null) _current.SetHighlight(false);
            if (cell != null) cell.SetHighlight(true);
            _current = cell;
        }

        // 하이라이트 정리 — 놓기·잡기 시작 시(Piece.OnReleased/OnGrabbed에서 호출).
        public void Clear()
        {
            if (_current != null) _current.SetHighlight(false);
            _current = null;
        }

        // 배치 가능 여부 — 빈 칸이고 슬롯이 락(오버소팅) 아닐 때만 하이라이트.
        private static bool CanHighlight(SlotCell cell)
        {
            return cell.IsEmpty
                && (cell.Slot == null || !cell.Slot.IsLocked);
        }

        // 호버 좌표의 셀 찾기 — Cell 레이어 raycast(Piece.FindCellAt과 동일 방식).
        private SlotCell FindCellAt(Vector2 worldPos)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos, _cellLayer);
            return hit != null ? hit.GetComponent<SlotCell>() : null;
        }
    }
}
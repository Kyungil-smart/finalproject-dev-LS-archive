using InGame.Slot;
using UnityEngine;

namespace InGame.Sort
{
    // 셀 하이라이트 — 드래그 중 기물이 호버한 셀을 강조("어느 칸에 들어갈지"). 항상 한 칸.
    //   셀 판정은 놓기와 동일한 SlotBoardManager 최근접 셀(거리) — 강조 셀과 실제 드롭 셀 일치.
    // 하이라이트 조건 — 호버 셀이 빈 칸 AND 슬롯 락 아님.
    // 작성자: 이성규
    public class CellHighlighter : MonoBehaviour
    {
        // 현재 하이라이트 중인 셀 — 한 칸만 유지. 호버 이동 시 직전 셀 끄고 새 셀 켬.
        private SlotCell _current;

        // 드래그 중 매 프레임 — 호버 셀 판정 후 하이라이트 갱신(Piece.OnDragging에서 호출).
        //   maxDist는 Piece의 스냅 반경을 그대로 받아 놓기와 동일 기준 유지.
        public void UpdateHover(Vector2 worldPos, float maxDist)
        {
            SlotCell cell = FindCellAt(worldPos, maxDist);

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
        private SlotCell FindCellAt(Vector2 worldPos,  float maxDist)
        {
            return SlotBoardManager.Instance!=null
                ? SlotBoardManager.Instance.FindNearestCell(worldPos, maxDist)
                : null;
        }
    }
}
using Core;
using UnityEngine;

namespace InGame.Sort
{
    // 3-Sort 기물. 드래그 가능한 오브젝트.
    // 1차 구현: 손가락 따라가기 + 놓으면 원위치 복귀.
    // 슬롯 시스템 진입 후 SortResult 발행·셀 진입 등 게임 로직 추가 예정.
    // 작성자: 이성규
    public class Piece : MonoBehaviour, IDraggable
    {
        // 드래그 시작 전 원래 위치 — 놓을 때 복귀 기준.
        private Vector3 _originalPos;

        // 터치 지점과 Piece 중심의 오프셋.
        // 중심으로 강제로 안 붙고, 잡은 지점이 손가락에 붙어 자연스럽게 따라옴.
        private Vector2 _dragOffset;

        public void OnGrabbed(Vector2 worldPos)
        {
            _originalPos = transform.position;
            _dragOffset = (Vector2)transform.position - worldPos;
        }

        public void OnDragging(Vector2 worldPos)
        {
            // z는 원래 값 유지 — 카메라 거리에 영향 받지 않게.
            Vector2 newPos = worldPos + _dragOffset;
            transform.position = new Vector3(newPos.x, newPos.y, _originalPos.z);
        }

        public void OnReleased(Vector2 worldPos)
        {
            // 1차에선 슬롯 시스템 없으니 무조건 원위치 복귀.
            // 슬롯 시스템 진입 후 ‒ 유효한 빈 셀이면 그 위치, 아니면 원위치 로직으로 교체.
            transform.position = _originalPos;
        }
    }
}
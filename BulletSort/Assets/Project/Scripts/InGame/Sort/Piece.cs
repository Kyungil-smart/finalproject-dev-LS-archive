using Core;
using UnityEngine;
using Logger = Core.Logger;

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
        
        [Header("References")]
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Collider2D _collider;
        
        [Header("Sorting Layer")]
        [Tooltip("드래그 중 기물이 올라갈 소팅 레이어")]
        [SerializeField] private SortingLayerType _draggingLayer = SortingLayerType.Dragging;
        
        // 드래그 시작 시 백업 — 놓을 때 복귀.
        private string _originalLayer;
        private int _originalOrder;
        
        void Awake()
        {
            if(_renderer == null) 
                _renderer = GetComponentInChildren<SpriteRenderer>();
            if(_collider == null) 
                _collider = GetComponent<Collider2D>();
        }
        
        public void OnGrabbed(Vector2 worldPos)
        {
            Logger.Instance.LogInfo($"{gameObject.name} OnGrabbed");
            
            _originalPos = transform.position;
            _dragOffset = (Vector2)transform.position - worldPos;
            
            // 소팅 백업 후 드래그 레이어로 올림 — 옆 슬롯 Frame·SlotUI 뒤로 안 숨음.
            _originalLayer = _renderer.sortingLayerName;
            _originalOrder = _renderer.sortingOrder;
            _renderer.sortingLayerName = _draggingLayer.ToName();
            
            // 자기 자신이 OverlapPoint에 잡히는 사고 차단.
            _collider.enabled = false;
        }
        
        public void OnDragging(Vector2 worldPos)
        {
            // z는 원래 값 유지 — 카메라 거리에 영향 받지 않게.
            Vector2 newPos = worldPos + _dragOffset;
            transform.position = new Vector3(newPos.x, newPos.y, _originalPos.z);
        }
        
        public void OnReleased(Vector2 worldPos)
        {
            Logger.Instance.LogInfo($"{gameObject.name} OnReleased");
            
            // 1차에선 슬롯 시스템 없으니 무조건 원위치 복귀.
            // 슬롯 시스템 진입 후 ‒ 유효한 빈 셀이면 그 위치, 아니면 원위치 로직으로 교체.
            transform.position = _originalPos;
            
            // 소팅·콜라이더 복귀.
            _renderer.sortingLayerName = _originalLayer;
            _renderer.sortingOrder = _originalOrder;
            _collider.enabled = true;
        }
    }
}
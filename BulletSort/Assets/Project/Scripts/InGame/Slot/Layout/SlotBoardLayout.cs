using System.Collections.Generic;
using Core;
using UnityEngine;

namespace InGame.Slot
{
    // 슬롯 보드 레이아웃 — 화면 가로 폭을 읽어 슬롯 9개를 3×3 그리드로 배치.
    // 월드 스페이스 스프라이트라 UGUI GridLayoutGroup을 못 써서 직접 계산.
    // UGUI GridLayoutGroup 방식 차용 — 패딩·스페이싱은 인스펙터, 셀 크기는 화면에서 역산.
    // "배치"만 담당 — 데이터·이벤트는 SlotBoardManager. 책임 분리.
    //
    // [동작 방식]
    // - 런타임: OnEnable에서 ScreenWatcher.OnResolutionChanged 구독 → 해상도 변경 시 재배치.
    // - 에디터(정지): ExecuteAlways + Update 폴링으로 Game 뷰 해상도 변경을 미리보기 반영,
    //   OnValidate로 인스펙터 값 변경 즉시 반영. (둘 다 #if UNITY_EDITOR — 빌드 제외)
    // - 셋업: 슬롯 9개를 SlotID 순서로 등록 + _slotBaseWidth에 슬롯 스프라이트 실제 폭 입력.
    // 작성자: 이성규
    [ExecuteAlways]  // 에디터 정지 상태에서도 Update 동작 — Game 뷰 해상도 변경 즉시 반영
    public class SlotBoardLayout : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("SlotID 순서로 등록된 슬롯 9개 (SlotBoardManager와 같은 순서)")]
        [SerializeField] private List<Transform> _slots;
        
        [Tooltip("기준 카메라 — 비우면 Camera.main 사용")]
        [SerializeField] private Camera _camera;
        
        [Header("Layout (UGUI GridLayoutGroup 방식)")]
        [Tooltip("화면 가로 폭 대비 좌우 패딩 비율. 0.05면 좌우 각 5%씩 여백을 둔다")]
        [Range(0f, 0.4f)]
        [SerializeField] private float _horizontalPaddingRatio = 0.1f;
        
        [Tooltip("슬롯 사이 간격을 셀 크기 대비 비율로. 0.15면 셀 폭의 15%만큼 띄운다")]
        [Range(0f, 1f)]
        [SerializeField] private float _spacingRatio = 0.278f;
        
        [Tooltip("보드 세로 중심을 화면 중앙(0) 기준 월드 단위로 이동. 양수면 위로, 음수면 아래로 (HUD 공간 확보용)")]
        [SerializeField] private float _verticalCenterOffset = 0f;
        
        [Tooltip("셀 한 칸의 가로:세로 비율. 1이면 정사각, 1.2면 세로로 길쭉한 칸")]
        [SerializeField] private float _cellAspect = 0.8f;
        
        [Header("Scale")]
        [Tooltip("셀 폭에 맞춰 슬롯 크기도 균등 스케일(비율 유지). 끄면 위치만 배치하고 크기는 원본 유지")]
        [SerializeField] private bool _scaleSlots = true;
        
        [Tooltip("스케일 1배 기준이 되는 슬롯 원본 가로 폭(월드 단위). 슬롯 스프라이트의 실제 폭을 넣는다")]
        [SerializeField] private float _slotBaseWidth = 2.64f;
        
        [Header("Auto Update")]
        [Tooltip("화면 크기 변경 시 자동 재배치. 런타임은 ScreenWatcher 이벤트로, 에디터(정지)는 Game 뷰 폴링으로 감지. 끄면 수동 Relayout·인스펙터 변경 때만 배치")]
        [SerializeField] private bool _autoRelayout = true;
        
        private void OnEnable()
        {
            Relayout();
            
            // 플레이 중에만 ScreenWatcher 구독 — 해상도 변경 이벤트로 재배치.
            // (에디터 정지 상태는 아래 OnValidate / ExecuteAlways Update로 처리)
            if (Application.isPlaying && _autoRelayout && ScreenWatcher.Instance != null)
                ScreenWatcher.Instance.OnResolutionChanged += Relayout;
        }
        
        private void OnDisable()
        {
            // ScreenWatcher가 먼저 파괴됐을 수 있음 (씬 종료 순서 보장 안 됨)
            if (Application.isPlaying && ScreenWatcher.Instance != null)
                ScreenWatcher.Instance.OnResolutionChanged -= Relayout;
        }

#if UNITY_EDITOR
        // 에디터 정지 상태 전용 — Game 뷰 해상도 변경 감지.
        // 플레이 중엔 ScreenWatcher 이벤트가 처리하므로 폴링 안 함.
        // 빌드에선 항상 isPlaying이라 불필요 — 컴파일 자체에서 제외.
        private Vector2Int _lastEditorScreenSize;
        private void Update()
        {
            if (Application.isPlaying) return;  // 런타임은 이벤트 구독으로 처리
            if (!_autoRelayout) return;
            
            var current = new Vector2Int(Screen.width, Screen.height);
            if (current != _lastEditorScreenSize)
            {
                _lastEditorScreenSize = current;
                Relayout();
            }
        }
        
        // 인스펙터 값 변경 시 에디터에서 즉시 반영
        private void OnValidate()
        {
            if (!Application.isPlaying)
                Relayout();
        }
#endif
        
        // 슬롯 9개를 화면 폭 기준 3×3으로 재배치.
        [ContextMenu("Relayout")]
        public void Relayout()
        {
            // 레이아웃할 슬롯이 없으면 종료
            if (_slots == null || _slots.Count == 0) return;

            // 카메라 확보 — 인스펙터 지정 우선, 없으면 Camera.main.
            // Awake 캐싱 대신 호출 시점에 찾음 — 에디터(OnValidate)에서도 동작하게.
            var cam = _camera != null ? _camera : Camera.main;
            if (cam == null || !cam.orthographic) return;
            
            // 1. 화면의 월드 단위 가로폭 = ortho size × aspect × 2
            float worldHeight = cam.orthographicSize * 2f;
            float worldWidth = worldHeight * cam.aspect;
            
            // 2. 패딩 제외한 사용 가능 폭
            float usableWidth = worldWidth * (1f - _horizontalPaddingRatio * 2f);

            int cols = Define.SLOT_BOARD_COLS;
            int rows = Define.SLOT_BOARD_ROWS;

            // 3. 셀 폭 역산 — 사용 폭 = 셀폭 × 열수 + 간격 × (열수-1)
            //    간격 = 셀폭 × spacingRatio 이므로
            //    usableWidth = cellW × (cols + spacingRatio × (cols-1))
            //    widthUnitCount = 사용 폭이 "셀 폭 몇 개분"인지 (셀 + 간격 환산)
            float widthUnitCount = cols + _spacingRatio * (cols - 1);
            float cellW = usableWidth / widthUnitCount;
            float cellH = cellW * _cellAspect;
            
            float spacingX = cellW * _spacingRatio;
            float spacingY = cellH * _spacingRatio;

            // 4. 그리드 전체 크기 — 중앙 정렬 기준점 계산용
            float gridW = cellW * cols + spacingX * (cols - 1);
            float gridH = cellH * rows + spacingY * (rows - 1);

            // 5. 화면 중앙 기준 — 좌상단 첫 셀 중심 위치
            Vector3 center = cam.transform.position;
            float startX = center.x - gridW * 0.5f + cellW * 0.5f;
            float startY = center.y + gridH * 0.5f - cellH * 0.5f + _verticalCenterOffset;

            // 6. 슬롯 크기 균등 스케일 비율 — 셀 폭 / 원본 폭. 비율 유지 위해 x·y 같은 값.
            float scale = 1f;
            if (_scaleSlots && _slotBaseWidth > 0f)
                scale = cellW / _slotBaseWidth;

            // 7. SlotID 순서대로 배치 (0~8, 행 우선: 0,1,2 윗줄 / 3,4,5 / 6,7,8)
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] == null) continue;
                
                int col = i % cols;
                int row = i / cols;
                
                float x = startX + col * (cellW + spacingX);
                float y = startY - row * (cellH + spacingY);
                
                var t = _slots[i];
                t.position = new Vector3(x, y, t.position.z);  // z는 보존
                
                // 비율 유지 균등 스케일 (z는 1로 — 2D라 의미 없음)
                if (_scaleSlots)
                    t.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }
}
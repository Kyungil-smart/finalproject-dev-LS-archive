using UnityEngine;

namespace Core.UI
{
    // 세이프 에리어 대응(고정 높이 띠 전용) — 좌우 stretch + 세로 고정 높이 오브젝트의
    //   anchoredPosition.y를 노치·홈 인디케이터 여백만큼 밀어 안전 영역 안으로.
    //   앵커·높이는 그대로라 자식 배치가 안 깨짐(계층 이동 불필요).
    //   상단 띠는 아래로, 하단 띠는 위로. 배경 패널엔 붙이지 않음(끝까지 꽉 차야 하므로).
    // 작성자: 이성규
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaOffset : MonoBehaviour
    {
        [Tooltip("상단 띠면 체크(노치만큼 아래로), 해제 시 하단 띠(인디케이터만큼 위로)")]
        [SerializeField] private bool _isTop = true;

        private RectTransform _rect;
        private float _basePosY;
        private Rect _lastSafeArea;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _basePosY = _rect.anchoredPosition.y;   // 원래 위치 백업
        }

        private void Start()
        {
            Apply();
            if (ScreenWatcher.Instance != null)
                ScreenWatcher.Instance.OnResolutionChanged += Apply;
        }

        private void OnDestroy()
        {
            if (ScreenWatcher.Instance != null)
                ScreenWatcher.Instance.OnResolutionChanged -= Apply;
        }

        // 세이프 에리어 여백(픽셀)을 캔버스 스케일로 환산해 anchoredPosition.y에 반영.
        private void Apply()
        {
            if (_rect == null || Screen.width <= 0 || Screen.height <= 0) return;

            Rect safe = Screen.safeArea;
            if (safe == _lastSafeArea) return;
            _lastSafeArea = safe;

            float padTop = Screen.height - safe.yMax;   // 노치 높이(픽셀)
            float padBottom = safe.yMin;                // 홈 인디케이터 높이(픽셀)

            var canvas = GetComponentInParent<Canvas>();
            float scale = (canvas != null && canvas.scaleFactor > 0f) ? canvas.scaleFactor : 1f;

            Vector2 pos = _rect.anchoredPosition;
            pos.y = _isTop
                ? _basePosY - (padTop / scale)      // 상단: 아래로
                : _basePosY + (padBottom / scale);  // 하단: 위로

            _rect.anchoredPosition = pos;
        }
    }
}
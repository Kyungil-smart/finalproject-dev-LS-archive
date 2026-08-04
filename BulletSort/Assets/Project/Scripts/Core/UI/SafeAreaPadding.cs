using UnityEngine;

namespace Core.UI
{
    // 세이프 에리어 패딩 — 원래 앵커·크기는 유지하고, 세이프 에리어만큼 오프셋만 밀어줌.
    //   계층 변경(컨테이너 삽입) 없이 기존 캔버스/컨테이너에 바로 부착 가능.
    //   상단 띠·하단 띠처럼 자체 앵커를 가진 오브젝트에 그대로 적용됨.
    // 작성자: 이성규
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPadding : MonoBehaviour
    {
        [Tooltip("적용할 방향 — 체크된 쪽만 세이프 에리어만큼 밈")]
        [SerializeField] private bool _top = true;
        [SerializeField] private bool _bottom = true;
        [SerializeField] private bool _left = false;
        [SerializeField] private bool _right = false;

        private RectTransform _rect;
        private Vector2 _baseOffsetMin;
        private Vector2 _baseOffsetMax;
        private Rect _lastSafeArea;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _baseOffsetMin = _rect.offsetMin;   // 원래 오프셋 백업
            _baseOffsetMax = _rect.offsetMax;
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

        // 세이프 에리어 여백(픽셀)을 캔버스 스케일로 환산해 오프셋에 더함.
        private void Apply()
        {
            if (_rect == null || Screen.width <= 0 || Screen.height <= 0) return;

            Rect safe = Screen.safeArea;
            if (safe == _lastSafeArea) return;
            _lastSafeArea = safe;

            // 화면 가장자리에서 안전 영역까지의 여백(픽셀)
            float padLeft   = safe.xMin;
            float padRight  = Screen.width - safe.xMax;
            float padBottom = safe.yMin;
            float padTop    = Screen.height - safe.yMax;

            // 캔버스 스케일로 환산(픽셀 → UI 단위)
            var canvas = GetComponentInParent<Canvas>();
            float scale = (canvas != null && canvas.scaleFactor > 0f) ? canvas.scaleFactor : 1f;

            _rect.offsetMin = _baseOffsetMin + new Vector2(
                _left   ? padLeft   / scale : 0f,
                _bottom ? padBottom / scale : 0f);

            _rect.offsetMax = _baseOffsetMax - new Vector2(
                _right ? padRight / scale : 0f,
                _top   ? padTop   / scale : 0f);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace Tutorial
{
    // GIF 대체 — 스프라이트 여러 장을 일정 간격으로 순회 재생.
    //   OnEnable에서 시작 / OnDisable에서 정지 → 스텝 토글에 자동 연동.
    //   대상 스텝 오브젝트의 자식으로 배치.
    // 작성자: 이성규
    public class SpriteLoopPlayer : MonoBehaviour
    {
        [Header("프레임")]
        [SerializeField] private Sprite[] _frames;        // 순회할 스프라이트(순서대로)
        [SerializeField] private float _interval = 0.5f;  // 프레임 간격(초)
        [SerializeField] private bool _loop = true;       // 마지막 후 처음으로 반복

        [Header("렌더 이미지")]
        [SerializeField] private Image _image;
        private int _frameIndex;
        private float _timer;

        private void Awake()
        {
            if (_image == null)
                _image = GetComponent<Image>();
        }

        // 켜질 때 처음 프레임부터 시작.
        private void OnEnable()
        {
            _frameIndex = 0;
            _timer = 0f;
            ApplyFrame();
        }

        private void Update()
        {
            if (_image == null)
                return;
            
            if (_frames == null || _frames.Length <= 1)
                return;

            _timer += Time.deltaTime;
            if (_timer < _interval)
                return;

            _timer -= _interval;
            Advance();
        }

        // 다음 프레임으로. loop 여부에 따라 마지막에서 정지 or 순환.
        private void Advance()
        {
            int next = _frameIndex + 1;

            if (next >= _frames.Length)
            {
                if (!_loop)
                    return;   // 반복 안 하면 마지막 프레임 유지
                next = 0;
            }

            _frameIndex = next;
            ApplyFrame();
        }

        // 현재 인덱스 스프라이트를 Image에 반영.
        private void ApplyFrame()
        {
            if (_image is not null &&_frames != null && _frameIndex < _frames.Length && _frames[_frameIndex] != null)
                _image.sprite = _frames[_frameIndex];
        }
    }
}
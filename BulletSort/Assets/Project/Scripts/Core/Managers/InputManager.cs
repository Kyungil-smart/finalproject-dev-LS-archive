using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    // 요약: 모바일 터치·마우스 입력을 받아 다운/드래그/업 이벤트를 발행하는 매니저.
    //       원시 입력 + 화면→월드 좌표 변환까지 책임지고, 그 이상(어떤 오브젝트가 잡혔는지 등)은 다루지 않는다.
    // 작성자: 이성규
    public class InputManager : Singleton<InputManager>
    {
        // GameInputActions: Input Action Asset에서 자동 생성한 C# 클래스.
        // 액션맵 Gameplay 안에 Press(Button), Point(Value Vector2) 두 액션이 있음.
        private GameInputActions _input;

        // 현재 누르고 있는 상태인지. Press.started ~ Press.canceled 사이에 true.
        private bool _isPressed;
        public bool IsPressed => _isPressed;

        // 외부 구독자(예: PointerHandler)는 이 이벤트들로 입력 흐름을 받음.
        // 모두 월드 좌표를 인자로 받으므로 구독자가 카메라 변환을 직접 처리할 필요 없음.
        public event Action<Vector2> OnPointerDown;
        public event Action<Vector2> OnPointerDrag;
        public event Action<Vector2> OnPointerUp;

        // 자식은 Awake를 건드리지 않고 Init()만 오버라이드 (Singleton<T> 합의 규약)
        protected override void Init()
        {
            _input = new GameInputActions();
            _input.Gameplay.Enable();

            // Press의 시작/종료 시점에서만 콜백 받음. 드래그(연속 발행)는 Update에서 처리.
            _input.Gameplay.Press.started += OnPressStarted;
            _input.Gameplay.Press.canceled += OnPressCanceled;

            Logger.Instance?.LogInfo("InputManager 초기화 완료");
        }

        // 매 프레임 누르고 있는 동안 드래그 이벤트 발행.
        // 손가락이 정지해 있어도 매 프레임 발행됨 — 구독자(PointerHandler 등)가
        // 이전 위치와의 차이를 검사할지 여부는 구독자 책임.
        private void Update()
        {
            if (_isPressed)
            {
                OnPointerDrag?.Invoke(ReadPointerWorld());
            }
        }

        private void OnPressStarted(InputAction.CallbackContext ctx)
        {
            _isPressed = true;
            OnPointerDown?.Invoke(ReadPointerWorld());
        }

        private void OnPressCanceled(InputAction.CallbackContext ctx)
        {
            _isPressed = false;
            OnPointerUp?.Invoke(ReadPointerWorld());
        }

        // 현재 포인터(터치 또는 마우스) 위치를 월드 좌표로 변환해 반환.
        private Vector2 ReadPointerWorld()
        {
            Vector2 screenPos = _input.Gameplay.Point.ReadValue<Vector2>();

            var cam = Camera.main;
            if (cam == null)
            {
                Logger.Instance?.LogError("InputManager: Camera.main을 찾지 못했습니다.");
                return Vector2.zero;
            }
            return cam.ScreenToWorldPoint(screenPos);
        }

        // 구독 해제 + InputActionAsset 해제로 메모리 누수 방지.
        // base.OnDestroy() 호출은 Singleton<T>의 _instance 정리를 위해 필수.
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_input != null)
            {
                _input.Gameplay.Press.started -= OnPressStarted;
                _input.Gameplay.Press.canceled -= OnPressCanceled;
                _input.Gameplay.Disable();
                _input.Dispose();
            }
        }
    }
}
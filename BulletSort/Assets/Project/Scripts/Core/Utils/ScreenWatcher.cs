using System;
using UnityEngine;

namespace Core
{
    // 요약: 화면 해상도·크기 변경을 감지해 이벤트로 발행하는 코어 매니저.
    //       Unity는 해상도 변경 콜백을 기본 제공하지 않아 폴링이 정석 —
    //       여러 컴포넌트가 각자 폴링하지 않도록 한 곳에서 감시하고 구독자에게 알린다.
    // 구독 예: SlotBoardLayout(슬롯 재배치), 적 스포너(경로·등장 위치 보정).
    // 작성자: 이성규
    public class ScreenWatcher : Singleton<ScreenWatcher>
    {
        // 화면 크기가 바뀐 프레임에 발행. 구독자는 자기 영역 재배치를 수행.
        public event Action OnResolutionChanged;
        
        // 마지막으로 감지한 화면 크기 — 변화 비교용.
        private Vector2Int _lastScreenSize;
        
        // 외부에서 현재 화면 크기 조회 (구독자가 즉시 한 번 맞출 때 사용).
        public Vector2Int CurrentScreenSize => new Vector2Int(Screen.width, Screen.height);
        
        // 자식은 Awake를 건드리지 않고 Init()만 오버라이드 (Singleton<T> 합의 규약)
        protected override void Init()
        {
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            Logger.Instance?.LogInfo("ScreenWatcher 초기화 완료");
        }
        
        // 매 프레임 화면 크기 비교 — struct 비교라 가벼움(힙 할당·GC 없음).
        // 변경된 프레임에만 이벤트 발행, 평소엔 비교만 하고 끝.
        private void Update()
        {
            var current = new Vector2Int(Screen.width, Screen.height);
            if (current == _lastScreenSize) return;
            
            _lastScreenSize = current;
            OnResolutionChanged?.Invoke();
        }
    }
}
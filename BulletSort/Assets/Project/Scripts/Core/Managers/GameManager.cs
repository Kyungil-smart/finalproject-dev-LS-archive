namespace Core
{
    // 요약: 프로젝트의 핵심 매니저들을 생성하고 관리하는 전역 진입점
    // 작성자: 이성규
    public class GameManager : Singleton<GameManager>
    {
        // 자식은 Awake를 건드리지 않고 Init()만 오버라이드
        // 이 Init()은 "살아남은 단 하나의 인스턴스"에서만 베이스(Singleton<T>)가 호출해 줍니다.
        protected override void Init()
        {
            // 1. 디버그 로거 초기화 로그 ( Logger는 씬에 프리팹으로 배치되어 있음 )
            if (Logger.Instance != null)
            {
                Logger.Instance.LogInfo("GameManager 초기화 시작");
            }

            // 2. 필요한 매니저들을 순서대로 등록 (생성하여 자식으로 붙임)
            // 주의: 다른 매니저가 데이터를 참조할 수 있으므로 DataManager를 가장 먼저 등록한다.
            // RegisterManager<DataManager>();
            RegisterManager<InputManager>();
            
            // TODO: (데모 단계 확정 시) 인게임 흐름, 3-Sort 판정 등 추가 매니저 등록
            // RegisterManager<InGameManager>();
            // RegisterManager<SortManager>();

            if (Logger.Instance != null)
            {
                Logger.Instance.LogInfo("GameManager 초기화 및 매니저 등록 완료");
            }
        }

        // --- 매니저 등록 헬퍼 --- //
        // 런타임에 T 타입의 싱글톤을 생성하고 GameManager의 자식으로 붙이는 역할
        private void RegisterManager<T>() where T : Singleton<T>
        {
            // Instance 프로퍼티를 호출하면 Singleton<T>의 자동 생성 로직이 동작함
            T manager = Singleton<T>.Instance;

            if (manager != null && manager.transform.parent != this.transform)
            {
                manager.transform.SetParent(this.transform);
            }
        }
    }
}
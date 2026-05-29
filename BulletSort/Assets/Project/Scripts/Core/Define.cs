namespace Core
{
    public static class Define
    {
        // 씬 이름
        public const string SCENE_LOBBY = "Lobby";
        public const string SCENE_INGAME = "InGame";
        public const string SCENE_RESULT = "Result";
        // 게임 진행 고정 규칙 (기획 확정값 — 변동 가능성 있으면 데이터로)
        public const int WAVE_PER_STAGE = 10;     // 스테이지당 웨이브 수
        public const int SORT_COUNT = 3;          // 정렬 완성에 필요한 기물 수
    }
    
    // 소팅 레이어 enum — 인스펙터에서 드롭다운으로 선택, 오타 차단
    public enum SortingLayerType
    {
        Board,
        Piece,
        Frame,
        SlotUI,    // 슬롯 안 월드 UI (HP·웨폰패널)
        Dragging,  // 드래그 중인 기물
        ScreenUI,  // HUD·모달 (CBT 단계)
    }
    
    // enum → Unity 소팅 레이어 이름(string) 매핑
    // enum 이름과 Tags&Layers 등록 이름이 별개임을 명시적으로 분리
    public static class SortingLayers
    {
        public static string ToName(this SortingLayerType type) => type switch
        {
            SortingLayerType.Board    => "Board",
            SortingLayerType.Piece    => "Piece",
            SortingLayerType.Frame    => "Frame",
            SortingLayerType.SlotUI   => "SlotUI",
            SortingLayerType.Dragging => "Dragging",
            SortingLayerType.ScreenUI => "ScreenUI",
            _ => "Default",
        };
    }
    
    public enum SceneType { Lobby, InGame, Result }
    public enum Language { KO, EN }       // 인게임 텍스트 시트 현지화
    public enum GamePhase { Wave, Intermission, Cleared, GameOver } // 인게임 흐름
}
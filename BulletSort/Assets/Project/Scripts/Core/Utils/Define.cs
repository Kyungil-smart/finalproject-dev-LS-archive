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
        
        // 슬롯·기물 배치 규칙 (기획 확정값 — 데이터 SO 도입 시 시트로 이관)
        public const int SORT_COUNT = 3;          // 정렬 완성에 필요한 기물 수 = 슬롯당 셀 수
        public const int PIECE_TYPE_COUNT = 3;    // 기물 종류 수 (데모 임시, CBT는 데이터)
        public const int PIECE_PER_TYPE = 9;      // 종류당 개수 (한 세트 기준)
        public const int REFILL_PER_SLOT = 2;     // 슬롯 보충 시 채우는 칸 수 (3칸 중 2칸)
        // 슬롯 보드 그리드 — 3×3 배치 (셀 수 SORT_COUNT)와 의미가 다른 별도 개념
        public const int SLOT_BOARD_COLS = 3; // 슬롯 보드 가로 슬롯 수
        public const int SLOT_BOARD_ROWS = 3; // 슬롯 보드 세로 슬롯 수
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
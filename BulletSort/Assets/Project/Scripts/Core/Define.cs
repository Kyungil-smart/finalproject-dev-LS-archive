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

    public enum SceneType { Lobby, InGame, Result }
    public enum Language { KO, EN }       // 인게임 텍스트 시트 현지화
    public enum GamePhase { Wave, Intermission, Cleared, GameOver } // 인게임 흐름
}
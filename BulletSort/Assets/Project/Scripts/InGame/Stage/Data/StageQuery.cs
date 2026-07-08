using System.Collections.Generic;
using Core;
using UnityEngine;

namespace InGame.Stage.Data
{
    // 스테이지 데이터 조회·가공 — 데이터 보유는 DataManager, 목록·이름→스프라이트 변환은 여기.
    //   배경은 둘로 분리: BGID=로비(스테이지 선택) 배경 / INGameBG=인게임 배경.
    //   StageIcon·BGID·INGameBG(string) → StageSpriteTable에서 Sprite로.
    // 전제: DataManager가 GetData<T>(id) 단건 + GetTable<T>() 전체 제공.
    // 작성자: 이성규
    public static class StageQuery
    {
        private static StageSpriteTable _spriteTable;
        private static StageSpriteTable SpriteTable =>
            _spriteTable != null ? _spriteTable
                : (_spriteTable = Resources.Load<StageSpriteTable>("SpriteTables/StageSpriteTable"));

        // StageID로 스테이지 데이터 조회. 없으면 null.
        public static StageData Get(int stageID)
        {
            return DataManager.Instance.GetData<StageData>(stageID);
        }

        // 전체 StageID 목록 — StageID 오름차순 정렬. 스테이지 선택 네비 순서용.
        public static IReadOnlyList<int> GetAllIDsSorted()
        {
            var table = DataManager.Instance.GetTable<StageData>();
            if (table == null) return new List<int>();

            var ids = new List<int>(table.Keys);
            ids.Sort();
            return ids;
        }

        // 스테이지 일러스트 — StageData.StageIcon 이름을 SpriteTable에서 변환.
        public static Sprite GetIcon(int stageID)
        {
            var data = Get(stageID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.StageIcon);
        }

        // 로비(스테이지 선택) 배경 — StageData.BGID. StageSelectController가 사용.
        public static Sprite GetLobbyBackground(int stageID)
        {
            var data = Get(stageID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.BGID);
        }

        // 인게임 배경 — StageData.INGameBG. 인게임 배경 스크립트가 StageManager 현재 StageID로 조회.
        public static Sprite GetInGameBackground(int stageID)
        {
            var data = Get(stageID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.INGameBG);
        }
    }
}
using System.Collections.Generic;
using Core;
using Lobby.Deck;
using UnityEngine;

namespace InGame.Sort.Data
{
    // 기물 데이터 조회·가공 — 데이터 보유는 DataManager(싱글톤, Resources)에 위임하고,
    // 도메인 고유 조회·가공(목록·연결 포탑·스프라이트 이름→객체 변환)은 여기가 담당.
    // 스프라이트는 PieceData가 이름(string)만 들고, 이 Query가 PieceSpriteTable에서 이름→Sprite 변환.
    // 전제: DataManager가 GetData<T>(id) 단건 + GetTable<T>() 전체를 제공.
    //
    // 데이터 구조 — 90개 = 이름 6 × 성급 3 × 레벨 5. 카드 단위는 (이름·성급) 그룹 18개이고
    //   그 안의 레벨 1~5가 강화 단계. 보유·현재레벨은 PieceInventory(런타임 상태)가 관리.
    // UpgradeCost 겸용 — Lv1 행 = 해금 비용(1성은 0), Lv2~5 행 = 그 레벨로 올리는 강화 비용.
    // ※ PieceType 필드 없음 — 타입은 연결 포탑(TowerData.TowerType)에서 유도.
    // 작성자: 이성규
    public static class PieceQuery
    {
        // 스프라이트 매핑 테이블 — 에셋 참조라 DataManager와 별개. Query가 직접 로드·캐싱.
        private static PieceSpriteTable _spriteTable;
        private static PieceSpriteTable SpriteTable =>
            _spriteTable != null ? _spriteTable
                : (_spriteTable = Resources.Load<PieceSpriteTable>("SpriteTables/PieceSpriteTable"));

        // 덱 카드 프레임·배경·유형아이콘 테이블 — 타입(1~6) 인덱스. 위 SpriteTable과 별개(키가 이름 아닌 타입).
        private static PieceCardTable _cardTable;
        private static PieceCardTable CardTable =>
            _cardTable != null ? _cardTable
                : (_cardTable = Resources.Load<PieceCardTable>("SpriteTables/PieceCardTable"));

        // ---- 그룹 인덱스 — (이름·성급) → 레벨별 PieceID. 첫 조회 시 1회 구축(테이블 불변). ----

        private class Group
        {
            public readonly Dictionary<int, int> LevelToID = new Dictionary<int, int>();
            public int MaxLevel = 1;
        }

        private static Dictionary<(string, int), Group> _groups;

        private static void EnsureGroups()
        {
            if (_groups != null) return;

            _groups = new Dictionary<(string, int), Group>();

            var table = DataManager.Instance.GetTable<PieceData>();
            if (table == null) return;

            foreach (var d in table.Values)
            {
                if (d == null) continue;

                var key = (d.PieceName, d.PieceGrade);
                if (!_groups.TryGetValue(key, out var g))
                    _groups[key] = g = new Group();

                g.LevelToID[d.PieceLv] = d.PieceID;
                if (d.PieceLv > g.MaxLevel) g.MaxLevel = d.PieceLv;
            }
        }

        // ---- 단건·목록 ----

        // PieceID로 기물 데이터 조회. 없으면 null (호출 측에서 빈 칸 처리).
        public static PieceData Get(int pieceID)
        {
            return DataManager.Instance.GetData<PieceData>(pieceID);
        }

        // 등록된 모든 PieceID(90개) — 레벨 포함 전체. 디버그·검증용.
        //   ※ 인게임 대기 그룹 폴백으로 쓰면 90종이라 3-Sort가 불가능. GetDefaultDeckIDs 사용할 것.
        public static IReadOnlyList<int> GetAllIDs()
        {
            var table = DataManager.Instance.GetTable<PieceData>();
            if (table == null) return new List<int>();

            var ids = new List<int>(table.Keys);
            ids.Sort();
            return ids;
        }

        // 보유 목록용 — 18개 그룹의 '현재 레벨' PieceID(미보유 포함, 표시는 호출부가 분기).
        //   강화 결과가 목록·인게임에 즉시 반영됨.
        public static IReadOnlyList<int> GetInventoryIDs()
        {
            EnsureGroups();
            PieceInventory.EnsureInit();

            var ids = new List<int>(_groups.Count);
            foreach (var kv in _groups)
            {
                var (name, grade) = kv.Key;
                int lv = PieceInventory.GetLevel(name, grade);

                if (kv.Value.LevelToID.TryGetValue(lv, out int id))
                    ids.Add(id);
            }

            ids.Sort();
            return ids;
        }

        // 기본 덱 6종 — 1성 그룹의 현재 레벨 ID. 인게임 대기 그룹 폴백(덱 없이 인게임 직접 실행)용.
        public static IReadOnlyList<int> GetDefaultDeckIDs()
        {
            EnsureGroups();
            PieceInventory.EnsureInit();

            var ids = new List<int>(6);
            foreach (var kv in _groups)
            {
                var (name, grade) = kv.Key;
                if (grade != 1) continue;

                int lv = PieceInventory.GetLevel(name, grade);
                if (kv.Value.LevelToID.TryGetValue(lv, out int id))
                    ids.Add(id);
            }

            ids.Sort();
            return ids;
        }

        // ---- 그룹 조회 (강화·해금) ----

        // 그룹의 특정 레벨에 해당하는 PieceID. 없으면 0.
        public static int GetIDByGroup(string name, int grade, int level)
        {
            EnsureGroups();
            if (_groups.TryGetValue((name, grade), out var g) && g.LevelToID.TryGetValue(level, out int id))
                return id;
            return 0;
        }

        // 그룹의 최대 레벨(데이터 기준). 강화 상한 판정용.
        public static int GetMaxLevel(string name, int grade)
        {
            EnsureGroups();
            return _groups.TryGetValue((name, grade), out var g) ? g.MaxLevel : 1;
        }

        // 해금 비용 — 그룹 Lv1 행의 UpgradeCost. (1성은 0 = 기본 해금)
        public static int GetUnlockCost(string name, int grade)
        {
            var d = Get(GetIDByGroup(name, grade, 1));
            return d != null ? d.UpgradeCost : 0;
        }

        // 다음 레벨 강화 비용 — (현재레벨+1) 행의 UpgradeCost. 상한이면 0.
        public static int GetNextUpgradeCost(string name, int grade, int curLevel)
        {
            var d = Get(GetIDByGroup(name, grade, curLevel + 1));
            return d != null ? d.UpgradeCost : 0;
        }

        // ---- 포탑·타입 ----

        // 기물 → 연결 포탑 데이터. 상세보기 스탯 표시 등에 사용. 없으면 null.
        public static TowerData GetTower(int pieceID)
        {
            var piece = Get(pieceID);
            if (piece == null) return null;
            return DataManager.Instance.GetData<TowerData>(piece.ConnectTower);
        }
        
        // 기물 → 연결 포탑 ID — 타워 소환 핸들러가 조회.
        public static int GetConnectTowerID(int pieceID)
        {
            var piece = Get(pieceID);
            return piece != null ? piece.ConnectTower : 0;
        }

        // 기물 → 연결 포탑 타입 — 슬롯 비주얼·덱 카드 프레임 등 '타입' 필요 지점의 단일 소스.
        public static int GetConnectTowerType(int pieceID)
        {
            var tower = GetTower(pieceID);
            return tower != null ? tower.TowerType : 0;
        }

        // ---- 스프라이트 ----

        // 기물 인게임 스프라이트 — PieceData.PieceSprite 이름을 SpriteTable에서 객체로 변환.
        public static Sprite GetSprite(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.PieceSprite);
        }

        // 덱 카드 초상화.
        public static Sprite GetPortrait(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.PiecePortrait);
        }

        // 캐릭터 이미지(강화창용).
        public static Sprite GetCartoon(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.PieceCartoon);
        }

        // 상세 일러스트(상세정보 팝업).
        public static Sprite GetDetailIllust(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.PieceDetailIllust);
        }

        // 덱 카드 프레임 — 타입별 동일.
        public static Sprite GetCardFrame(int pieceID)
        {
            if (CardTable == null) return null;
            return CardTable.GetFrame(GetConnectTowerType(pieceID));
        }

        // 덱 카드 배경 — 타입별 동일.
        public static Sprite GetCardBackground(int pieceID)
        {
            if (CardTable == null) return null;
            return CardTable.GetBackground(GetConnectTowerType(pieceID));
        }

        // 덱 카드 공격 유형 아이콘 — 타입별 동일.
        public static Sprite GetTypeIcon(int pieceID)
        {
            if (CardTable == null) return null;
            return CardTable.GetTypeIcon(GetConnectTowerType(pieceID));
        }
    }
}
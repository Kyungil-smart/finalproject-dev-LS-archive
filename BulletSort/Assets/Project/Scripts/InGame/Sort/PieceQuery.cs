using System.Collections.Generic;
using Core;
using UnityEngine;

namespace InGame.Sort.Data
{
    // 기물 데이터 조회·가공 — 데이터 보유는 DataManager(싱글톤, Resources)에 위임하고,
    // 도메인 고유 조회·가공(목록·연결 포탑·스프라이트 이름→객체 변환)은 여기가 담당.
    // 스프라이트는 PieceData가 이름(string)만 들고, 이 Query가 PieceSpriteTable에서 이름→Sprite 변환.
    // 전제: DataManager가 GetData<T>(id) 단건 + GetTable<T>() 전체를 제공.
    // ※ 최신 파싱본 반영 — PieceType 필드 제거(타입은 연결 포탑에서 유도).
    //   초상화 전용 컬럼 부재로 GetPortrait는 PieceCartoon 임시 차용(시트 정리 후 교체).
    //   PieceCartoon·PieceDetailIllust는 상세정보·강화창용.
    // 작성자: 이성규
    public static class PieceQuery
    {
        // 스프라이트 매핑 테이블 — 에셋 참조라 DataManager와 별개. Query가 직접 로드·캐싱.
        private static PieceSpriteTable _spriteTable;
        private static PieceSpriteTable SpriteTable =>
            _spriteTable != null ? _spriteTable
                : (_spriteTable = Resources.Load<PieceSpriteTable>("SpriteTables/PieceSpriteTable"));

        // 덱 카드 프레임·배경 테이블 — 타입(1~6) 인덱스. 위 SpriteTable과 별개(키가 이름 아닌 타입).
        private static PieceCardTable _cardTable;
        private static PieceCardTable CardTable =>
            _cardTable != null ? _cardTable
                : (_cardTable = Resources.Load<PieceCardTable>("SpriteTables/PieceCardTable"));

        // PieceID로 기물 데이터 조회. 없으면 null (호출 측에서 빈 칸 처리).
        public static PieceData Get(int pieceID)
        {
            return DataManager.Instance.GetData<PieceData>(pieceID);
        }

        // 등록된 모든 PieceID 목록 — 공급기가 대기 그룹 생성 시 사용(레벨 포함 전체).
        public static IReadOnlyList<int> GetAllIDs()
        {
            var table = DataManager.Instance.GetTable<PieceData>();
            return table != null ? new List<int>(table.Keys) : new List<int>();
        }

        // 카드 단위 대표 ID — 덱 보유 목록용. 같은 (이름·성급)은 한 장의 카드이고 레벨은 그 카드의 상태.
        //   레벨마다 따로 뜨지 않게 (이름·성급) 그룹의 최저 레벨을 대표로 뽑음. ID 오름차순 정렬.
        //   정식 보유/레벨은 동적(김경민 데이터) — 그때 대표를 보유 레벨 ID로 바꾸면 됨.
        public static IReadOnlyList<int> GetRepresentativeIDs()
        {
            var table = DataManager.Instance.GetTable<PieceData>();
            if (table == null) return new List<int>();

            // (이름, 성급) → 그 그룹 최저 레벨 대표
            var reps = new Dictionary<(string, int), PieceData>();
            foreach (var data in table.Values)
            {
                if (data == null) continue;
                var key = (data.PieceName, data.PieceGrade);
                if (!reps.TryGetValue(key, out var cur) || data.PieceLv < cur.PieceLv)
                    reps[key] = data;
            }

            var ids = new List<int>(reps.Count);
            foreach (var rep in reps.Values)
                ids.Add(rep.PieceID);
            ids.Sort();
            return ids;
        }

        // 기물 → 연결 포탑 ID — 타워 소환 핸들러가 조회.
        public static int GetConnectTowerID(int pieceID)
        {
            var data = Get(pieceID);
            return data != null ? data.ConnectTower : 0;
        }

        // 기물 → 연결 포탑 타입 — 슬롯 비주얼·덱 카드 프레임 등 '타입' 필요 지점의 단일 소스.
        //   PieceData에 PieceType 필드가 없어졌으므로, 타입은 연결 포탑(TowerData.TowerType)에서 유도.
        public static int GetConnectTowerType(int pieceID)
        {
            var piece = Get(pieceID);
            if (piece == null) return 0;

            var tower = DataManager.Instance.GetData<TowerData>(piece.ConnectTower);
            return tower != null ? tower.TowerType : 0;
        }

        // 기물 인게임 스프라이트 — PieceData.PieceSprite 이름을 SpriteTable에서 객체로 변환.
        public static Sprite GetSprite(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.PieceSprite);
        }

        // 덱 카드 초상화 — 전용 컬럼 부재로 임시로 PieceCartoon 차용. 컬럼 도착 시 이 한 줄만 교체.
        public static Sprite GetPortrait(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.PieceCartoon);
        }

        // 캐릭터 이미지(강화창용) — PieceData.PieceCartoon 이름을 SpriteTable에서 변환. 소비처는 추후.
        public static Sprite GetCartoon(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.PieceCartoon);
        }

        // 상세 일러스트(상세정보) — PieceData.PieceDetailIllust 이름을 SpriteTable에서 변환. 소비처는 추후.
        public static Sprite GetDetailIllust(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.PieceDetailIllust);
        }

        // 덱 카드 프레임 — 타입별 동일. 타입 소스는 연결 포탑(GetConnectTowerType).
        public static Sprite GetCardFrame(int pieceID)
        {
            if (CardTable == null) return null;
            return CardTable.GetFrame(GetConnectTowerType(pieceID));
        }

        // 덱 카드 배경 — 타입별 동일. 타입 소스는 연결 포탑(GetConnectTowerType).
        public static Sprite GetCardBackground(int pieceID)
        {
            if (CardTable == null) return null;
            return CardTable.GetBackground(GetConnectTowerType(pieceID));
        }
    }
}
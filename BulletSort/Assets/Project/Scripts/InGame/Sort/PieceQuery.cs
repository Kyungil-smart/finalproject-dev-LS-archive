using System.Collections.Generic;
using Core;
using InGame.Tower.Data;
using UnityEngine;

namespace InGame.Sort.Data
{
    // 기물 데이터 조회·가공 — 데이터 보유는 DataManager(싱글톤, Resources)에 위임하고,
    // 도메인 고유 조회·가공(목록·연결 포탑·스프라이트 이름→객체 변환)은 여기가 담당.
    // 스프라이트는 PieceData가 이름(string)만 들고, 이 Query가 PieceSpriteTable에서 이름→Sprite 변환.
    //   → 호출부(Piece)는 GetSprite(id)만 부르면 됨. 이름 조회든 직접참조든 내부에 숨김.
    //   → 추후 어드레서블 전환 시 SpriteTable 로드 한 줄만 교체.
    // 전제: DataManager가 GetData<T>(id) 단건 + GetTable<T>() 전체를 제공.
    // 작성자: 이성규
    public static class PieceQuery
    {
        // 스프라이트 매핑 테이블 — 에셋 참조라 DataManager(CSV 자동 적재)와 별개. Query가 직접 로드·캐싱.
        // 추후 어드레서블 전환 시 이 로드만 교체(Resources.Load → Addressables).
        private static PieceSpriteTable _spriteTable;
        private static PieceSpriteTable SpriteTable =>
            _spriteTable != null ? _spriteTable
                : (_spriteTable = Resources.Load<PieceSpriteTable>("SpriteTables/PieceSpriteTable"));
        
        // PieceID로 기물 데이터 조회. 없으면 null (호출 측에서 빈 칸 처리).
        public static PieceData Get(int pieceID)
        {
            return DataManager.Instance.GetData<PieceData>(pieceID);
        }

        // 등록된 모든 PieceID 목록 — 공급기가 대기 그룹 생성 시 사용.
        // DataManager의 전체 테이블에서 키만 추림.
        public static IReadOnlyList<int> GetAllIDs()
        {
            var table = DataManager.Instance.GetTable<PieceData>();
            return table != null ? new List<int>(table.Keys) : new List<int>();
        }

        // 기물 → 연결 포탑 ID — 타워 소환 핸들러가 조회.
        public static int GetConnectTowerID(int pieceID)
        {
            var data = Get(pieceID);
            return data != null ? data.ConnectTower : 0;
        }
        
        // 기물 → 연결 포탑 타입 — 슬롯 비주얼이 조회.
        public static int GetConnectTowerType(int pieceID)
        {
            var piece = Get(pieceID);
            if (piece == null) return 0;

            var tower = DataManager.Instance.GetData<TowerData>(piece.ConnectTower);
            return tower != null ? tower.TowerType : 0;
        }
        
        // 기물 인게임 스프라이트 — PieceData의 이름(PieceSprite)을 SpriteTable에서 객체로 변환.
        // Piece.SetByID가 호출. 데이터에 이름만 있고 객체 참조가 없어도 호출부는 이 한 줄로 끝.
        public static Sprite GetSprite(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.PieceSprite);
        }
        
        // 기물 초상화 — PieceData의 Portrait(이름)를 SpriteTable에서 객체로 변환. 덱 카드용.
        public static Sprite GetPortrait(int pieceID)
        {
            var data = Get(pieceID);
            if (data == null || SpriteTable == null) return null;
            return SpriteTable.GetByName(data.Portrait);
        }
    }
}
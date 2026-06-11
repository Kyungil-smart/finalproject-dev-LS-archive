using System.Collections.Generic;
using Core;

namespace InGame.Sort.Data
{
    // 기물 데이터 조회 — DataManager가 보유한 PieceData 테이블에 위임.
    // 데이터 보유는 DataManager(싱글톤, Resources 로드)가, 도메인 고유 조회는 여기가 담당.
    // 데이터 출처만 DataManager로 옮기고 조회 인터페이스(Get/GetAllIDs)는 유지해 호출부(Piece·공급기) 영향 최소화.
    // 전제: DataManager가 GetData<T>(int id) 단건 + GetTable<T>() 전체 테이블 두 기반을 제공.
    //       (GetTable은 데이터 담당과 협의해 추가 — 도메인 Query들이 목록·필터에 사용)
    // 작성자: 이성규
    public static class PieceQuery
    {
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
            return data != null ? data.ConnectTowerID : 0;
        }
    }
}
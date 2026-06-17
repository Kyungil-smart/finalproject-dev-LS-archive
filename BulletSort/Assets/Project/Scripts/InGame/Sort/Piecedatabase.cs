using System.Collections.Generic;
using UnityEngine;

namespace InGame.Sort.Data
{
    // PieceID → PieceData 조회용 모음 SO. 임시 — 데이터 담당의 DataManager·정식 DB가 들어오면
    // 그쪽 조회로 교체. 지금은 Piece가 enum·Sprite[] 배열 대신 이 DB를 거치게 해서,
    // 나중에 조회 출처만 바꾸면 되도록 구조를 미리 잡아둠.
    // 사용: Piece 프리팹(또는 보드 매니저)에 이 SO 하나를 꽂아두고 GetByID(pieceID)로 조회.
    // 프리팹 1개에만 꽂으면 27개 풀링 인스턴스가 같은 DB를 공유.
    // 현재는 사용하지 않는 레거시 코드
    // 작성자: 이성규
    // [CreateAssetMenu(fileName = "PieceDatabase", menuName = "Scriptable Objects/Temp/PieceDatabase")]
    public class PieceDatabase : ScriptableObject
    {
        [Tooltip("기물 데이터 목록 — PieceID 순서 무관, GetByID가 ID로 찾음")]
        [SerializeField] private PieceData[] pieces;
        
        // PieceID → PieceData 캐시. 첫 조회 시 1회 구성.
        private Dictionary<int, PieceData> _lookup;
        
        // PieceID로 기물 데이터 조회. 없으면 null (호출 측에서 None 처리).
        public PieceData GetByID(int pieceID)
        {
            BuildLookupIfNeeded();
            return _lookup.TryGetValue(pieceID, out var data) ? data : null;
        }
        
        // 등록된 모든 PieceID 목록 — 공급기가 대기 그룹 생성 시 사용.
        // ID 체계(8001 등)를 데이터가 정하므로, 공급기는 이 목록만 받아 채운다.
        public IReadOnlyList<int> GetAllIDs()
        {
            BuildLookupIfNeeded();
            return new List<int>(_lookup.Keys);
        }

        private void BuildLookupIfNeeded()
        {
            if(_lookup != null) return; // 이미 만들었으면 스킵 (1회만)
            
            _lookup = new Dictionary<int, PieceData>();
            if(pieces == null) return;

            foreach (var piece in pieces)
            {
                if (piece == null) continue; // 인스펙터 빈 칸 가드
                // 중복 ID는 첫 항목 우선 — 인스펙터 실수 방지 가드.
                if(!_lookup.ContainsKey(piece.PieceID))
                    _lookup.Add(piece.PieceID, piece);
            }
        }
    }
}

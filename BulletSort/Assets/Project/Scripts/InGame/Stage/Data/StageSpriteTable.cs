using System.Collections.Generic;
using UnityEngine;

namespace InGame.Stage.Data
{
    // 스테이지 스프라이트 매핑 테이블 — 이름(string) → Sprite.
    //   StageData가 일러스트·배경을 이름(StageIcon·BGID)으로만 들고, StageQuery가 이 표에서 객체로 변환.
    //   PieceSpriteTable과 동일 패턴. 에셋은 Resources/SO/StageSpriteTable/ 아래(테이블별 폴더 컨벤션).
    // 작성자: 이성규
    [CreateAssetMenu(fileName = "StageSpriteTable", menuName = "Scriptable Objects/StageSpriteTable")]
    public class StageSpriteTable : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public string name;      // StageData의 StageIcon·BGID와 일치
            public Sprite sprite;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();

        // 이름→Sprite 조회 캐시 — 첫 조회 시 1회 구축.
        private Dictionary<string, Sprite> _map;

        // 이름으로 스프라이트 조회. 없으면 null(호출부에서 빈 이미지 처리).
        public Sprite GetByName(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;

            if (_map == null)
            {
                _map = new Dictionary<string, Sprite>(_entries.Count);
                foreach (var e in _entries)
                    if (!string.IsNullOrEmpty(e.name) && e.sprite != null)
                        _map[e.name] = e.sprite;
            }

            return _map.TryGetValue(spriteName, out var sprite) ? sprite : null;
        }
    }
}
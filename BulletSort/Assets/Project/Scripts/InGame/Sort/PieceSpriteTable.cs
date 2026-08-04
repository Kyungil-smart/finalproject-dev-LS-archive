using System.Collections.Generic;
using UnityEngine;

namespace InGame.Sort.Data
{
    // 스프라이트 이름 → Sprite 매핑 SO. PieceData는 이름(string)만 들고,
    // 실제 Sprite 객체는 여기서 이름으로 조회(데이터/에셋 분리). PieceQuery가 이 테이블을 들고 가공.
    // 자동 생성 대상 아님 — 에셋 참조는 사람이 꽂는 개인 SO. 인게임 스프라이트·초상화 모두 등록 가능.
    // 작성자: 이성규
    [CreateAssetMenu(fileName = "PieceSpriteTable", menuName = "Scriptable Objects/PieceSpriteTable")]
    public class PieceSpriteTable : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public string Name;     // PieceData.PieceSprite / Portrait와 매칭되는 이름
            public Sprite Sprite;
        }

        [Tooltip("이름→스프라이트 매핑. 기물 인게임 스프라이트·초상화 모두 등록")]
        [SerializeField] private Entry[] _entries;

        private Dictionary<string, Sprite> _lookup;

        // 이름으로 스프라이트 조회. 첫 호출 시 딕셔너리 1회 구성(PieceDatabase와 같은 패턴).
        public Sprite GetByName(string spriteName)
        {
            BuildLookupIfNeeded();
            if (string.IsNullOrEmpty(spriteName)) return null;
            return _lookup.TryGetValue(spriteName, out var sp) ? sp : null;
        }

        private void BuildLookupIfNeeded()
        {
            if (_lookup != null) return;
            _lookup = new Dictionary<string, Sprite>(_entries.Length);
            foreach (var e in _entries)
            {
                if (string.IsNullOrEmpty(e.Name) || e.Sprite == null) continue;
                if (!_lookup.ContainsKey(e.Name))
                    _lookup[e.Name] = e.Sprite;
                else
                    Debug.LogWarning($"PieceSpriteTable : 중복 이름 {e.Name}");
            }
        }
    }
}
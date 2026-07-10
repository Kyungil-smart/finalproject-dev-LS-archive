using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Core.UI
{
    // 한 테이블 엔트리에 여러 문구가 묶여 있을 때, 원하는 조각만 뽑아 TMP에 넣는다.
    //   예) Language_Button = "<변경><취소>" → index 0 → "변경" / index 1 → "취소"
    //   ko/en/ja 모두 같은 개수의 <> 토큰을 갖는다는 전제.
    // 배선 — LocalizeStringEvent의 Update String을 TMP.text가 아니라 이 컴포넌트의
    //   OnStringChanged(string)로 연결. 이 스크립트가 파싱 후 _target에 대입.
    // ※ 임시 대응 — 원칙은 엔트리를 하나씩 나누는 것(Language_Confirm / Language_Cancel).
    //   시트를 못 건드리는 동안의 우회책.
    // 작성자: 이성규
    public class LocalizedSegmentText : MonoBehaviour
    {
        [Tooltip("문구를 넣을 TMP. 비우면 같은 오브젝트에서 찾음")]
        [SerializeField] private TMP_Text _target;

        [Tooltip("몇 번째 <> 조각인가. 0부터")]
        [SerializeField] private int _index;

        [Tooltip("조각이 없거나 인덱스를 벗어나면 원문 그대로 표시")]
        [SerializeField] private bool _fallbackToRaw = true;

        // <...> 안의 내용만 캡처. 꺾쇠 자체는 버림.
        private static readonly Regex TokenPattern = new Regex(@"<([^<>]*)>", RegexOptions.Compiled);

        private void Awake()
        {
            if (_target == null) _target = GetComponent<TMP_Text>();
        }

        // LocalizeStringEvent.UpdateString이 호출 — 언어 전환 시마다 새 문자열이 들어온다.
        public void OnStringChanged(string localized)
        {
            if (_target == null) return;
            _target.text = Extract(localized);
        }

        // <> 토큰을 잘라 _index번째를 반환. 토큰이 없으면 원문(단일 문구 엔트리 호환).
        private string Extract(string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;

            var matches = TokenPattern.Matches(source);

            if (matches.Count == 0)
                return _fallbackToRaw ? source : string.Empty;

            if (_index < 0 || _index >= matches.Count)
            {
                Debug.LogWarning($"[LocalizedSegment] index {_index} 범위 밖 (토큰 {matches.Count}개) — {name}");
                return _fallbackToRaw ? source : string.Empty;
            }

            return matches[_index].Groups[1].Value;
        }
    }
}
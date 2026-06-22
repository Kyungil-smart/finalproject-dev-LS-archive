using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    // 하단 탭바 — 탭 클릭 시 (윈도우 전환 + 탭 활성표시 + 상단 라벨 갱신)을 한 번에.
    // 탭 하나에 묶이는 요소를 TabEntry로 묶어 인덱스로 일괄 토글.
    // 작성자: 이성규
    public class LobbyTabBar : MonoBehaviour
    {
        [Serializable]
        public struct TabEntry
        {
            public Button Button;          // 탭 버튼 (투명 클릭 영역)
            public GameObject ActiveImage;  // 활성 표시(자식 그라데이션 포함)
            public GameObject Window;       // 대응 중앙 윈도우
            public string LabelText;        // 상단 라벨 텍스트 (배틀/덱 편성 등)
            public Sprite LabelIcon;        // 상단 라벨 아이콘
        }

        [Header("Tabs")]
        [SerializeField] private TabEntry[] _tabs;

        [Header("TopInfo (상단 라벨)")]
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private Image _labelIcon;

        [Header("초기 선택 탭")]
        [SerializeField] private int _defaultIndex = 0;  // 메인 진입 = 배틀

        private int _current = -1;

        private void Awake()
        {
            // 각 탭 버튼에 자기 인덱스로 선택 콜백 등록
            for (int i = 0; i < _tabs.Length; i++)
            {
                int index = i;  // 클로저 캡처 — 루프 변수 직접 쓰면 마지막 값으로 고정
                if (_tabs[i].Button != null)
                    _tabs[i].Button.onClick.AddListener(() => Select(index));
            }
        }

        private void Start()
        {
            Select(_defaultIndex);
        }

        // 탭 선택 — 윈도우·활성표시·상단 라벨 일괄 전환
        public void Select(int index)
        {
            if (index < 0 || index >= _tabs.Length) return;
            if (index == _current) return;  // 같은 탭 재선택 무시

            for (int i = 0; i < _tabs.Length; i++)
            {
                bool on = (i == index);
                if (_tabs[i].Window != null) _tabs[i].Window.SetActive(on);
                if (_tabs[i].ActiveImage != null) _tabs[i].ActiveImage.SetActive(on);
            }

            // 상단 라벨 갱신
            var tab = _tabs[index];
            if (_labelText != null) _labelText.text = tab.LabelText;
            if (_labelIcon != null && tab.LabelIcon != null) _labelIcon.sprite = tab.LabelIcon;

            _current = index;
        }
    }
}
using System;
using Lobby.Deck;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 정렬(필터) 팝업 — 보유 상태 1택 + 유형 다중 선택. 확인 시 onApply로 필터 전달.
    //   보유 라디오 3개는 ToggleGroup으로 묶어 1택(Allow Switch Off 해제).
    //   유형이 전부 켜지면 마스터(전체 유형 보기) ON, 하나라도 꺼지면 OFF. 마스터를 누르면 전부 ON/OFF.
    //   딤은 PopupManager가 공용 관리 — 확인은 PopupBase.Close로 닫아 OnClosed를 발행시킴.
    // 작성자: 이성규
    public class SortPopup : PopupBase
    {
        [Header("보유 필터 (ToggleGroup, 1택)")]
        [SerializeField] private Toggle _allToggle;        // 전체
        [SerializeField] private Toggle _ownedToggle;      // 보유
        [SerializeField] private Toggle _notOwnedToggle;   // 미보유

        [Header("유형 필터")]
        [Tooltip("전체 유형 보기 — 마스터 체크")]
        [SerializeField] private Toggle _allTypeToggle;

        [Tooltip("유형 체크 6개. 인덱스 0~5 = PieceType 1~6 (PieceCardTable._typeIcons와 같은 순서)")]
        [SerializeField] private Toggle[] _typeToggles;

        [Header("확인")]
        [SerializeField] private Button _confirmButton;

        private Action<SortFilter> _onApply;

        // 마스터↔개별 토글 상호 갱신 중 콜백 재귀 방지
        private bool _syncing;

        // 자식 초기화 훅 — PopupBase.Awake가 _root 세팅 후 호출. 여기서 버튼·토글 리스너 등록.
        protected override void OnAwake()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirm);

            if (_allTypeToggle != null)
                _allTypeToggle.onValueChanged.AddListener(OnAllTypeChanged);

            if (_typeToggles != null)
                foreach (var t in _typeToggles)
                    if (t != null) t.onValueChanged.AddListener(_ => OnTypeChanged());
        }

        // ---- 열기 ----

        // 현재 필터로 초기 표시. 확인 시 onApply로 새 필터 전달.
        public void Show(SortFilter current, Action<SortFilter> onApply)
        {
            _onApply = onApply;

            _syncing = true;   // 초기 세팅 중 콜백 무시
            ApplyOwnedToUI(current);
            ApplyTypesToUI(current);
            _syncing = false;

            SyncMaster();
            Open();
        }

        private void ApplyOwnedToUI(SortFilter current)
        {
            if (_allToggle != null) _allToggle.isOn = current.Owned == OwnedFilter.All;
            if (_ownedToggle != null) _ownedToggle.isOn = current.Owned == OwnedFilter.Owned;
            if (_notOwnedToggle != null) _notOwnedToggle.isOn = current.Owned == OwnedFilter.NotOwned;
        }

        private void ApplyTypesToUI(SortFilter current)
        {
            if (_typeToggles == null) return;

            // 필터가 비어 있으면(= 전체) 유형 전부 켬
            bool allTypes = current.Types.Count == 0;

            for (int i = 0; i < _typeToggles.Length; i++)
                if (_typeToggles[i] != null)
                    _typeToggles[i].isOn = allTypes || current.Types.Contains(i + 1);
        }

        // ---- 토글 동기화 ----

        // 마스터 체크 → 유형 전부 ON/OFF
        private void OnAllTypeChanged(bool on)
        {
            if (_syncing || _typeToggles == null) return;

            _syncing = true;
            foreach (var t in _typeToggles)
                if (t != null) t.isOn = on;
            _syncing = false;
        }

        // 개별 유형 변경 → 마스터 상태 갱신
        private void OnTypeChanged()
        {
            if (_syncing) return;
            SyncMaster();
        }

        // 유형이 전부 켜졌으면 마스터 ON, 하나라도 꺼졌으면 OFF
        private void SyncMaster()
        {
            if (_allTypeToggle == null || _typeToggles == null) return;

            bool all = true;
            foreach (var t in _typeToggles)
                if (t != null && !t.isOn) { all = false; break; }

            _syncing = true;
            _allTypeToggle.isOn = all;
            _syncing = false;
        }

        // ---- 확인 ----

        // 현재 UI 상태를 필터로 만들어 전달하고 닫음.
        private void OnConfirm()
        {
            _onApply?.Invoke(BuildFilter());
            Close();   // PopupBase — OnClosed 발행 → PopupManager가 딤 정리
        }

        private SortFilter BuildFilter()
        {
            var filter = new SortFilter();

            if (_ownedToggle != null && _ownedToggle.isOn) filter.Owned = OwnedFilter.Owned;
            else if (_notOwnedToggle != null && _notOwnedToggle.isOn) filter.Owned = OwnedFilter.NotOwned;
            else filter.Owned = OwnedFilter.All;

            // 마스터가 켜져 있으면 Types를 비워둠(= 전체 유형). 일부만 켜졌으면 그 유형만 담음.
            bool allTypes = _allTypeToggle != null && _allTypeToggle.isOn;
            if (!allTypes && _typeToggles != null)
                for (int i = 0; i < _typeToggles.Length; i++)
                    if (_typeToggles[i] != null && _typeToggles[i].isOn)
                        filter.Types.Add(i + 1);

            return filter;
        }
    }
}
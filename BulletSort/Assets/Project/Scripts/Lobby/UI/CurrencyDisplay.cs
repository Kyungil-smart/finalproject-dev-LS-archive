using Reward;
using TMPro;
using UnityEngine;

namespace Lobby.UI
{
    // 상단 재화 표시 — 골드·스타더스트. RewardManager 변경 이벤트를 구독해 즉시 갱신.
    //   TopInfo_Canvas/Currency_Info에 부착. 로비 어느 탭에서 소비해도 여기서 반영됨.
    // OnRewardDataChanged가 static event라 OnDisable에서 반드시 해제(씬 전환 시 죽은 참조 방지).
    // 작성자: 이성규
    public class CurrencyDisplay : MonoBehaviour
    {
        [Tooltip("Gold_Info 안의 숫자 텍스트")]
        [SerializeField] private TMP_Text _goldText;

        [Tooltip("Stardust_Info 안의 숫자 텍스트")]
        [SerializeField] private TMP_Text _stardustText;

        private void OnEnable()
        {
            RewardManager.OnRewardDataChanged += OnChanged;
            Refresh();   // 구독 전 이미 로드됐을 수 있어 즉시 1회
        }

        private void OnDisable()
        {
            RewardManager.OnRewardDataChanged -= OnChanged;
        }

        private void OnChanged(RewardManager.RewardSaveData _) => Refresh();

        // 현재 보유량으로 표시 갱신. 매니저가 아직 없으면(초기화 순서) 조용히 넘어감.
        private void Refresh()
        {
            var mgr = RewardManager.Instance;
            if (mgr == null || mgr.CurrentData == null) return;

            if (_goldText != null) _goldText.text = mgr.CurrentData.Gold.ToString("N0");
            if (_stardustText != null) _stardustText.text = mgr.CurrentData.StarDust.ToString("N0");
        }
    }
}
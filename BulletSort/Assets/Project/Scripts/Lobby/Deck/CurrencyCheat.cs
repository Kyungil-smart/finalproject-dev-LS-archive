using Reward;
using UnityEngine;

namespace Lobby.Deck
{
    // 개발용 치트 — 재화 지급·인벤토리 초기화. 에디터 전용(#if UNITY_EDITOR)이라 빌드에 안 들어감.
    //   씬의 아무 오브젝트에 붙이고, 인스펙터 컴포넌트 톱니바퀴 메뉴에서 호출.
    //   키 입력 대신 ContextMenu — Input System 충돌 회피 + 실수 방지.
    // AddReward가 NotifyAndSave를 부르므로 상단 재화 표시·카드 라벨이 즉시 갱신됨.
    // 작성자: 이성규
    public class CurrencyCheat : MonoBehaviour
    {
        [Tooltip("지급량 — 아래 메뉴가 이 값을 사용")]
        [SerializeField] private int _amount = 1000;

        [ContextMenu("골드 지급")]
        private void AddGold()
        {
            if (!IsPlaying()) return;
            RewardManager.Instance.AddReward(_amount, 0);
            Debug.Log($"[Cheat] 골드 +{_amount}");
        }

        [ContextMenu("스타더스트 지급")]
        private void AddStardust()
        {
            if (!IsPlaying()) return;
            RewardManager.Instance.AddReward(0, _amount);
            Debug.Log($"[Cheat] 스타더스트 +{_amount}");
        }

        [ContextMenu("둘 다 지급")]
        public void AddBoth()
        {
            if (!IsPlaying()) return;
            RewardManager.Instance.AddReward(_amount, _amount);
            Debug.Log($"[Cheat] 골드·스타더스트 +{_amount}");
        }

        [ContextMenu("인벤토리 초기화 (1성만 보유, 전부 Lv1)")]
        private void ResetInventory()
        {
            if (!IsPlaying()) return;
            PieceInventory.ResetAll();
            Debug.Log("[Cheat] 인벤토리 초기화");
        }

        [ContextMenu("세이브 경로 출력")]
        private void PrintSavePath()
        {
            Debug.Log($"[Cheat] persistentDataPath: {Application.persistentDataPath}");
        }

        // 플레이 중에만 동작 — 에디트 모드에선 매니저가 없어 NRE.
        private bool IsPlaying()
        {
            if (Application.isPlaying && RewardManager.Instance != null) return true;
            Debug.LogWarning("[Cheat] 플레이 중에만 사용 가능합니다.");
            return false;
        }
    }
}
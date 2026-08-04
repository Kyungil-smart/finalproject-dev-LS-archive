using Lobby;
using UnityEngine;

namespace Tutorial
{
    // 스텝 진입 시 로비 탭바를 지정 탭으로 전환.
    //   LobbyTabBar.Select 하나가 윈도우 전환 + 탭 하이라이트 + 상단 라벨을 일괄 처리.
    //   튜토리얼은 언더바 레이캐스터를 제거해 클릭이 안 오므로, 스텝에서 Select를 직접 호출.
    //   프리팹에 부착, _tabBar 참조·_tabIndex는 씬에서 할당(로비 셸은 씬 인스턴스).
    // 작성자: 이성규
    public class TutorialTabSelect : MonoBehaviour
    {
        [SerializeField] private LobbyTabBar _tabBar;   // 로비 셸 탭바
        [SerializeField] private int _tabIndex;         // 이 스텝에서 강조/전환할 탭

        private void OnEnable()
        {
            if (_tabBar != null)
                _tabBar.Select(_tabIndex);
        }
    }
}
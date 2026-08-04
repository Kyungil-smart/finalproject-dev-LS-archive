using UnityEngine;

namespace Tutorial
{
    // 스텝 진입 시 로비 셸(Window_Canvas)의 윈도우를 전환.
    //   튜토리얼 스텝은 별도 캔버스라, 아래 로비 셸 오브젝트는 SetActive 자동 연동이 안 됨 → 이 스크립트가 토글.
    //   대상 스텝 오브젝트에 부착. OnEnable에서 켤 것/끌 것 처리.
    // 작성자: 이성규
    public class TutorialWindowSwitch : MonoBehaviour
    {
        [Header("전환 대상 (로비 Window_Canvas 아래)")]
        [SerializeField] private GameObject[] _show;   // 켤 윈도우(예: 덱편성)
        [SerializeField] private GameObject[] _hide;   // 끌 윈도우(예: 배틀)

        private void OnEnable()
        {
            foreach (var go in _hide)
                if (go != null) go.SetActive(false);

            foreach (var go in _show)
                if (go != null) go.SetActive(true);
        }
    }
}
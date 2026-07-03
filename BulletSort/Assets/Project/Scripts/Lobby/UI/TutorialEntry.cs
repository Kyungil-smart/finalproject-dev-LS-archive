using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lobby.UI
{
    // 로비 ? 버튼 → 튜토리얼 진입 확인 팝업 → 확인 시 튜토리얼 씬 로드.
    //   버튼→PopupManager 연결은 SettingController 패턴과 동일(코드 AddListener).
    // 작성자: 이성규
    public class TutorialEntry : MonoBehaviour
    {
        [Header("진입 버튼")]
        [SerializeField] private Button _helpButton;   // 로비 ? 버튼

        private void Start()
        {
            if (_helpButton != null)
                _helpButton.onClick.AddListener(OnTapHelp);
        }

        // ? 버튼 → 확인/취소 팝업. 확인 시 튜토리얼 씬 진입, 취소는 닫기만.
        private void OnTapHelp()
        {
            PopupManager.Instance.ShowAlert(
                "튜토리얼을 확인하시겠어요?",
                OnConfirmEnter,   // 확인 → 씬 진입
                () => { }         // 취소 → 닫기만(빈 콜백으로 취소 버튼 ON)
            );
        }

        // 튜토리얼 씬 진입.
        private void OnConfirmEnter()
        {
            SceneManager.LoadScene(Define.SCENE_TUTORIAL);
        }
    }
}
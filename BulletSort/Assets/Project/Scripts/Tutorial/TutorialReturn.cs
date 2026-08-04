using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tutorial
{
    // 튜토리얼 → 로비 복귀. 플레이스홀더 단계 임시 복귀 버튼용.
    //   정식 TutorialFlow 완성 시 마지막 페이지 복귀 로직으로 흡수 예정.
    // 작성자: 이성규
    public class TutorialReturn : MonoBehaviour
    {
        [Header("복귀 버튼")]
        [SerializeField] private Button _returnButton;   // 로비로 귀환 버튼

        private void Start()
        {
            if (_returnButton != null)
                _returnButton.onClick.AddListener(OnTapReturn);
        }

        // 로비 씬으로 복귀.
        private void OnTapReturn()
        {
            SceneManager.LoadScene(Define.SCENE_LOBBY);
        }
    }
}
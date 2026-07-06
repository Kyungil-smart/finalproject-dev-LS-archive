using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tutorial
{
    // 튜토리얼 진행 — 전체 화면 버튼 클릭마다 다음 스텝으로.
    //   스텝 오브젝트를 순서대로 인스펙터 등록, 현재 인덱스만 SetActive.
    //   마지막 스텝 이후 클릭 시 완료 팝업 → 확인하면 로비 복귀.
    // 작성자: 이성규
    public class TutorialFlow : MonoBehaviour
    {
        [Header("진행")]
        [SerializeField] private Button _screenButton;   // 최상위 전체 화면 투명 버튼
        [SerializeField] private GameObject[] _steps;    // 세부 스텝(순서대로)

        private int _index = -1;

        private void Start()
        {
            if (_screenButton != null)
                _screenButton.onClick.AddListener(OnTapNext);

            // 시작: 첫 스텝만 표시
            ShowStep(0);
        }

        // 화면 클릭 → 다음 스텝. 마지막이면 완료 팝업.
        private void OnTapNext()
        {
            int next = _index + 1;

            if (next >= _steps.Length)
            {
                ShowFinishPopup();
                return;
            }

            ShowStep(next);
        }

        // 지정 인덱스만 켜고 나머지 끔.
        private void ShowStep(int index)
        {
            for (int i = 0; i < _steps.Length; i++)
            {
                if (_steps[i] != null)
                    _steps[i].SetActive(i == index);
            }

            _index = index;
        }

        // 튜토리얼 완료 팝업 → 확인 시 로비 복귀.
        private void ShowFinishPopup()
        {
            PopupManager.Instance.ShowAlert(
                "튜토리얼을 완료했어요!",
                OnConfirmFinish   // 확인 → 로비 복귀
            );
        }

        // 로비 씬으로 복귀.
        private void OnConfirmFinish()
        {
            SceneManager.LoadScene(Define.SCENE_LOBBY);
        }
    }
}
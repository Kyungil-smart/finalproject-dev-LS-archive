using Core;
using InGame.Stage.Data;
using UnityEngine;
using UnityEngine.UI;

namespace InGame
{
    // 인게임 배경 — StageManager의 현재 StageID로 인게임 배경(INGameBG)을 조회해 깖.
    //   스테이지 선택(로비)이 SetStageID로 넘긴 스테이지가 곧 이 배경. 로비 배경(BGID)과는 다른 필드.
    //   인게임 씬 배경 Image에 부착. 데이터 없으면 기존 이미지 유지(빈 화면 방지).
    // 작성자: 이성규
    [RequireComponent(typeof(Image))]
    public class InGameBackground : MonoBehaviour
    {
        [SerializeField] private Image _background;   // 비우면 Awake에서 자기 Image

        private void Awake()
        {
            if (_background == null)
                _background = GetComponent<Image>();
        }

        private void Start()
        {
            Apply();
        }

        // 현재 스테이지의 인게임 배경 적용.
        private void Apply()
        {
            if (_background == null) return;
            if (StageManager.Instance == null) return;

            int stageID = StageManager.Instance.CurStageID;
            Debug.Log(stageID);

            Sprite bg = StageQuery.GetInGameBackground(stageID);
            Debug.Log(bg.name);
            if (bg != null)
                _background.sprite = bg;   // 없으면 기존 유지
        }
    }
}
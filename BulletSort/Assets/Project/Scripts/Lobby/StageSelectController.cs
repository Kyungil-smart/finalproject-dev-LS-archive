using System.Collections.Generic;
using Core;
using DG.Tweening;
using InGame.Stage.Data;
using Lobby.Deck;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    // 스테이지 선택(배틀 탭) — 이전/다음 순회, 현재 스테이지의 번호·부제·일러스트·배경 갱신.
    //   일러스트: A/B 2장 크로스 슬라이드(방향성). 배경: A/B 2장 크로스페이드(제자리 블렌딩).
    //   배경은 로비용(BGID) — GetLobbyBackground. 인게임 배경(INGameBG)은 인게임 스크립트가 별도 조회.
    //   시작 → 현재 StageID를 StageManager에 저장 후 DeckBuilder.OnTapStart 위임(6칸 체크·덱 수집·진입).
    //   ※ 덱 확인 팝업·클리어 마크는 이어서/패스. 슬라이드 거리는 영역 폭 기준(해상도 대응).
    // 작성자: 이성규
    public class StageSelectController : MonoBehaviour
    {
        [Header("네비 버튼")]
        [SerializeField] private Button _prevButton;   // 이전 스테이지
        [SerializeField] private Button _nextButton;   // 다음 스테이지
        [SerializeField] private Button _startButton;  // 시작

        [Header("제목")]
        [SerializeField] private TMP_Text _numberText;   // 맨 위 스테이지 번호(숫자만, '스테이지'는 고정 텍스트 분리)
        [SerializeField] private TMP_Text _titleText;    // 부제(StageName)

        [Header("일러스트 (슬라이드 2장)")]
        [Tooltip("슬라이드 이동 영역 — 이 폭만큼 좌우로 밀어 화면 밖으로. RectMask2D 걸어 밖 잘라내기")]
        [SerializeField] private RectTransform _slideArea;
        [SerializeField] private Image _illustA;
        [SerializeField] private Image _illustB;

        [Header("배경 (크로스페이드 2장, 로비 캔버스)")]
        [SerializeField] private Image _bgA;
        [SerializeField] private Image _bgB;

        [Header("시작 위임")]
        [Tooltip("6칸 체크·덱 수집·인게임 진입 담당. 배틀 탭에서 덱 편성 탭의 DeckBuilder 참조(씬 할당)")]
        [SerializeField] private DeckBuilder _deckBuilder;

        [Header("연출")]
        [Tooltip("슬라이드·크로스페이드 시간(초)")]
        [SerializeField] private float _duration = 0.3f;
        [Tooltip("전환 이징")]
        [SerializeField] private Ease _ease = Ease.OutCubic;

        private IReadOnlyList<int> _stageIDs;
        private int _index;

        // 현재 앞면(표시 중) 일러스트/배경. 전환 때 앞뒤 역할을 스왑.
        private Image _illustFront;
        private Image _bgFront;

        private bool _transitioning;   // 전환 중 입력 중복 차단

        // 현재 선택 StageID — 범위 밖이면 0.
        private int CurrentStageID =>
            (_stageIDs != null && _index >= 0 && _index < _stageIDs.Count) ? _stageIDs[_index] : 0;

        private void Start()
        {
            _stageIDs = StageQuery.GetAllIDsSorted();

            if (_prevButton != null) _prevButton.onClick.AddListener(OnPrev);
            if (_nextButton != null) _nextButton.onClick.AddListener(OnNext);
            if (_startButton != null) _startButton.onClick.AddListener(OnStart);

            // 앞면 초기 지정 — A를 앞, B를 뒤(대기)로.
            _illustFront = _illustA;
            _bgFront = _bgA;

            _index = 0;
            InitFirstStage();   // 첫 진입은 연출 없이 앞면에 즉시 세팅
        }

        private void OnDestroy()
        {
            // 씬 나갈 때 진행 중 트윈 정리(파괴 오브젝트 트윈 경고 방지)
            _illustA?.DOKill(); _illustB?.DOKill();
            _bgA?.DOKill(); _bgB?.DOKill();
            (_illustA?.rectTransform)?.DOKill();
            (_illustB?.rectTransform)?.DOKill();
        }

        // 첫 스테이지 — 앞면 이미지에 즉시 세팅, 뒷면은 숨김.
        private void InitFirstStage()
        {
            UpdateNavButtons();

            int id = CurrentStageID;
            if (id == 0) return;

            SetTitle(id);

            // 앞면에 현재 스테이지, 뒷면은 투명·중앙 대기
            ApplySprite(_illustFront, StageQuery.GetIcon(id), 1f);
            ApplySprite(Other(_illustFront), null, 0f);
            CenterInstant(_illustFront);

            ApplySprite(_bgFront, StageQuery.GetLobbyBackground(id), 1f);
            ApplySprite(Other(_bgFront), null, 0f);
        }

        // 이전 — 왼쪽에서 들어옴(현재는 오른쪽으로 나감).
        private void OnPrev()
        {
            if (_transitioning || _index <= 0) return;
            _index--;
            Transition(fromRight: false);
        }

        // 다음 — 오른쪽에서 들어옴(현재는 왼쪽으로 나감).
        private void OnNext()
        {
            if (_transitioning || _stageIDs == null || _index >= _stageIDs.Count - 1) return;
            _index++;
            Transition(fromRight: true);
        }

        // 전환 — 일러스트 크로스 슬라이드 + 배경 크로스페이드.
        //   fromRight=true(다음): 새 일러스트가 오른쪽 밖 → 중앙, 현재는 왼쪽 밖으로.
        private void Transition(bool fromRight)
        {
            int id = CurrentStageID;
            if (id == 0) return;

            _transitioning = true;
            UpdateNavButtons();   // 전환 시작 시 버튼 상태 갱신(끝단 즉시 반영)
            SetTitle(id);

            float width = _slideArea != null ? _slideArea.rect.width : Screen.width;
            float dir = fromRight ? 1f : -1f;   // 다음이면 새 이미지 오른쪽(+), 나가는 건 왼쪽(-)

            // --- 일러스트 슬라이드 ---
            Image inImg = Other(_illustFront);   // 들어올(뒷면) 이미지
            Image outImg = _illustFront;         // 나갈(앞면) 이미지

            ApplySprite(inImg, StageQuery.GetIcon(id), 1f);
            SetAnchoredX(inImg, dir * width);    // 들어올 이미지: 화면 밖(방향쪽)에 배치
            inImg.transform.SetAsLastSibling();  // 위로

            inImg.rectTransform.DOKill();
            outImg.rectTransform.DOKill();
            inImg.rectTransform.DOAnchorPosX(0f, _duration).SetEase(_ease);
            outImg.rectTransform.DOAnchorPosX(-dir * width, _duration).SetEase(_ease)
                .OnComplete(() =>
                {
                    _illustFront = inImg;   // 앞뒤 스왑
                    _transitioning = false;
                });

            // --- 배경 크로스페이드 (제자리, 알파만) ---
            Image bgIn = Other(_bgFront);
            Image bgOut = _bgFront;

            ApplySprite(bgIn, StageQuery.GetLobbyBackground(id), 0f);
            bgIn.transform.SetAsLastSibling();

            bgIn.DOKill();
            bgOut.DOKill();
            bgIn.DOFade(1f, _duration).SetEase(_ease);
            bgOut.DOFade(0f, _duration).SetEase(_ease)
                .OnComplete(() => { _bgFront = bgIn; });
        }

        // 번호(_index+1) + 부제(StageName) 갱신. "스테이지" 고정 글자는 별도 오브젝트라 안 건드림.
        private void SetTitle(int stageID)
        {
            if (_numberText != null)
                _numberText.text = (_index + 1).ToString();

            if (_titleText != null)
            {
                var data = StageQuery.Get(stageID);
                if (data != null) _titleText.text = data.StageName;
            }
        }

        // 스프라이트 + 알파 적용. 스프라이트 없으면 이미지 끔.
        private void ApplySprite(Image img, Sprite sprite, float alpha)
        {
            if (img == null) return;

            img.sprite = sprite;
            img.enabled = sprite != null;

            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }

        // 일러스트 즉시 중앙 배치(연출 없이).
        private void CenterInstant(Image img)
        {
            if (img != null) SetAnchoredX(img, 0f);
        }

        private void SetAnchoredX(Image img, float x)
        {
            if (img == null) return;
            Vector2 p = img.rectTransform.anchoredPosition;
            p.x = x;
            img.rectTransform.anchoredPosition = p;
        }

        // A/B 짝 반환 — 앞면 주면 뒷면 돌려줌.
        private Image Other(Image img)
        {
            if (img == _illustA) return _illustB;
            if (img == _illustB) return _illustA;
            if (img == _bgA) return _bgB;
            if (img == _bgB) return _bgA;
            return null;
        }

        // 첫/마지막에서 이전/다음 버튼 숨김(SetActive). 반투명 비활성 아니라 아예 안 보이게.
        private void UpdateNavButtons()
        {
            int count = _stageIDs?.Count ?? 0;

            if (_prevButton != null) _prevButton.gameObject.SetActive(_index > 0);
            if (_nextButton != null) _nextButton.gameObject.SetActive(_index < count - 1);
        }

        // 시작 — 현재 StageID 저장 후 DeckBuilder에 위임(6칸 체크·덱 수집·진입).
        //   덱 확인 팝업은 이어서 — 그때 SetStageID와 위임 사이에 팝업을 끼움.
        private void OnStart()
        {
            int id = CurrentStageID;
            if (id == 0) return;

            StageManager.Instance.SetStageID(id);

            if (_deckBuilder != null)
                _deckBuilder.OnTapStart();
        }
    }
}
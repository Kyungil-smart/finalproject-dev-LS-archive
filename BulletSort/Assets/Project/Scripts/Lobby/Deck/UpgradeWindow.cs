using System.Collections.Generic;
using Core;
using InGame.Sort.Data;
using Reward;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby.Deck
{
    // 강화 윈도우 — 하단 목록(18그룹, DeckCard 재사용)에서 하나 골라 상단 상세 표시 + 해금/강화.
    //   미보유 → 버튼 "해금하기", 비용 = Lv1 행 UpgradeCost, 재화 = 스타더스트
    //   보유    → 버튼 "강화하기", 비용 = 다음 레벨 행 UpgradeCost, 재화 = 골드
    //   최대 레벨 → 버튼 비활성("MAX"), 비용 영역 숨김
    // 카드 상단 라벨 — 선택=Select / 미보유=Buy / 강화가능=Level Up / 최대=없음.
    // 성급 선행 조건(1성→2성→3성)은 미도입 — 재화 비용이 사실상 게이트 역할.
    // 재화 소비는 RewardManager(팀원). 성공 시 PieceInventory 갱신 → OnChanged로 목록·상세 재표시.
    // 선택 상태는 PieceID가 아니라 그룹 키(이름·성급)로 보관 — 강화하면 PieceID가 바뀌기 때문.
    // 작성자: 이성규
    public class UpgradeWindow : MonoBehaviour
    {
        [Header("목록")]
        [Tooltip("OwnedList/Scroll View/Viewport/Content")]
        [SerializeField] private Transform _cardContent;
        [SerializeField] private DeckCard _cardPrefab;

        [Tooltip("UnitOwned_Info/UnitCount_Text — (n명)")]
        [SerializeField] private TMP_Text _ownedCountText;

        [Header("캐릭터")]
        [SerializeField] private Image _cartoonImage;   // Cartoon_Area/Cartoon_Image
        [SerializeField] private Image _pieceImage;     // Cartoon_Area/Piece_Visual/Piece_Image

        [Header("정보 — TopInfo")]
        [SerializeField] private TMP_Text _nameText;    // "이름"
        [SerializeField] private TMP_Text _starText;    // "(1성)"
        [SerializeField] private TMP_Text _levelText;   // "Lv. 1"

        [Header("정보 — 스탯 (TowerData)")]
        [SerializeField] private TMP_Text _typeText;      // Type/Type_Info_Text
        [SerializeField] private TMP_Text _ammoText;      // Ammo/Type_Info_Text
        [SerializeField] private TMP_Text _damageText;    // Damage/Damge_Text
        [SerializeField] private TMP_Text _rangeText;     // Range/Range_Text
        [SerializeField] private TMP_Text _fireRateText;  // FireRate/Damge_Text

        [Header("해금 / 강화 — Upgrade_Area")]
        [Tooltip("비용 영역 — 최대 레벨이면 숨김")]
        [SerializeField] private GameObject _costArea;
        [SerializeField] private Image _costIcon;        // Stardust_Icon (골드/스타더스트 교체)
        [SerializeField] private TMP_Text _costText;     // "50/120" (보유/필요)

        [SerializeField] private Button _actionButton;   // Button
        [SerializeField] private TMP_Text _actionLabel;  // Act_Text

        [Header("재화 아이콘")]
        [SerializeField] private Sprite _goldIcon;
        [SerializeField] private Sprite _stardustIcon;

        [Header("카드 상단 라벨")]
        [SerializeField] private Sprite _labelSelect;    // "Select"
        [SerializeField] private Sprite _labelLevelUp;   // "Level Up"
        [SerializeField] private Sprite _labelBuy;       // "Buy"

        private readonly List<DeckCard> _cards = new List<DeckCard>();

        // 선택 그룹 — 레벨은 인벤토리에서 매번 조회.
        private string _selName;
        private int _selGrade = -1;

        // ---- 생명주기 ----

        private void Start()
        {
            if (_actionButton != null) _actionButton.onClick.AddListener(OnTapAction);

            BuildList();
            SelectFirst();
        }

        private void OnEnable()
        {
            PieceInventory.OnChanged += OnInventoryChanged;
            RewardManager.OnRewardDataChanged += OnRewardChanged;
        }

        private void OnDisable()
        {
            PieceInventory.OnChanged -= OnInventoryChanged;
            RewardManager.OnRewardDataChanged -= OnRewardChanged;
        }

        // ---- 목록 ----

        // 18개 그룹 카드 생성. 탭하면 선택(편성 아님).
        private void BuildList()
        {
            foreach (var id in PieceQuery.GetInventoryIDs())
            {
                var card = Instantiate(_cardPrefab, _cardContent);
                card.Setup(id, OnTapCard);
                _cards.Add(card);
            }
        }

        // 첫 보유 카드를 기본 선택 — 빈 상세 방지.
        private void SelectFirst()
        {
            foreach (var card in _cards)
                if (card.IsOwned) { OnTapCard(card); return; }

            if (_cards.Count > 0) OnTapCard(_cards[0]);
        }

        private void OnTapCard(DeckCard card)
        {
            var data = PieceQuery.Get(card.PieceID);
            if (data == null) return;

            _selName = data.PieceName;
            _selGrade = data.PieceGrade;

            RefreshCards();
            RefreshDetail();
        }

        // 인벤토리 변경(해금·강화) → 카드 재Setup(레벨·미보유 갱신) + 전체 재표시.
        //   재생성하지 않아 스크롤 위치 유지.
        private void OnInventoryChanged()
        {
            var ids = PieceQuery.GetInventoryIDs();
            for (int i = 0; i < _cards.Count && i < ids.Count; i++)
                _cards[i].Setup(ids[i], OnTapCard);

            RefreshCards();
            RefreshDetail();
        }

        // 재화 변경 → 카드·상세의 보유량 표시 갱신.
        private void OnRewardChanged(RewardManager.RewardSaveData _)
        {
            RefreshCards();
            RefreshAction();
        }

        // ---- 카드 표시 ----

        // 카드별 선택 강조·상단 라벨·하단 비용 갱신.
        //   라벨 — 선택은 항상 Select. 그 외엔 '지금 실행 가능한 것'만 표시.
        //     미보유 + 스타더스트 충분 → Buy
        //     보유 + 강화 여지 + 골드 충분 → Level Up
        //     그 외(재화 부족·최대 레벨) → 라벨 없음
        //   비용은 라벨과 무관하게, 실행 대상이 있으면 항상 표시(부족해도 얼마인지 보여줌).
        private void RefreshCards()
        {
            var reward = RewardManager.Instance != null ? RewardManager.Instance.CurrentData : null;
            int gold = reward != null ? reward.Gold : 0;
            int dust = reward != null ? reward.StarDust : 0;

            int owned = 0;

            foreach (var card in _cards)
            {
                var d = PieceQuery.Get(card.PieceID);
                if (d == null) continue;

                if (card.IsOwned) owned++;

                bool sel = d.PieceName == _selName && d.PieceGrade == _selGrade;
                card.SetSelected(sel);

                int level = PieceInventory.GetLevel(d.PieceName, d.PieceGrade);
                int max = PieceQuery.GetMaxLevel(d.PieceName, d.PieceGrade);

                if (!card.IsOwned)
                {
                    int cost = PieceQuery.GetUnlockCost(d.PieceName, d.PieceGrade);
                    card.SetCost(_stardustIcon, dust, cost);

                    // 살 수 있을 때만 Buy
                    card.SetStatusLabel(sel ? _labelSelect : (dust >= cost ? _labelBuy : null));
                }
                else if (level < max)
                {
                    int cost = PieceQuery.GetNextUpgradeCost(d.PieceName, d.PieceGrade, level);
                    card.SetCost(_goldIcon, gold, cost);

                    // 강화할 수 있을 때만 Level Up
                    card.SetStatusLabel(sel ? _labelSelect : (gold >= cost ? _labelLevelUp : null));
                }
                else
                {
                    // 최대 레벨 — 비용도 라벨도 없음
                    card.HideCost();
                    card.SetStatusLabel(sel ? _labelSelect : null);
                }
            }

            if (_ownedCountText != null) _ownedCountText.text = $"({owned}명)";
        }

        // ---- 상세 ----

        private void RefreshDetail()
        {
            if (_selGrade < 0) return;

            int level = PieceInventory.GetLevel(_selName, _selGrade);
            int pieceID = PieceQuery.GetIDByGroup(_selName, _selGrade, level);

            if (_cartoonImage != null) _cartoonImage.sprite = PieceQuery.GetCartoon(pieceID);
            if (_pieceImage != null) _pieceImage.sprite = PieceQuery.GetSprite(pieceID);

            if (_nameText != null) _nameText.text = _selName;
            if (_starText != null) _starText.text = $"({_selGrade}성)";
            if (_levelText != null) _levelText.text = $"Lv. {level}";

            SetStats(pieceID);
            RefreshAction();
        }

        // 전투 수치는 연결 포탑에서. 없으면 대시.
        private void SetStats(int pieceID)
        {
            var tower = PieceQuery.GetTower(pieceID);
            if (tower == null)
            {
                SetText(_typeText, "-");
                SetText(_ammoText, "-");
                SetText(_damageText, "-");
                SetText(_rangeText, "-");
                SetText(_fireRateText, "-");
                return;
            }

            SetText(_typeText, tower.TowerTypeText);
            SetText(_ammoText, $"{tower.TowerMaxAmmo}발");
            SetText(_damageText, tower.TowerAtk.ToString());
            SetText(_rangeText, tower.TowerMaxRange.ToString("0.##"));
            SetText(_fireRateText, $"{tower.TowerAtkSpeed:0.00}s");
        }

        private static void SetText(TMP_Text t, string v)
        {
            if (t != null) t.text = v;
        }

        // ---- 해금 / 강화 버튼 ----

        // 보유 여부·레벨에 따라 버튼 라벨·비용·활성 상태를 결정.
        private void RefreshAction()
        {
            if (_selGrade < 0) return;

            var reward = RewardManager.Instance != null ? RewardManager.Instance.CurrentData : null;

            bool owned = PieceInventory.IsOwned(_selName, _selGrade);
            int level = PieceInventory.GetLevel(_selName, _selGrade);
            int max = PieceQuery.GetMaxLevel(_selName, _selGrade);

            // 미보유 → 해금(스타더스트)
            if (!owned)
            {
                int cost = PieceQuery.GetUnlockCost(_selName, _selGrade);
                ShowAction("해금하기", _stardustIcon, reward != null ? reward.StarDust : 0, cost);
                return;
            }

            // 보유 + 최대 레벨 → 비활성
            if (level >= max)
            {
                if (_costArea != null) _costArea.SetActive(false);
                if (_actionLabel != null) _actionLabel.text = "MAX";
                if (_actionButton != null) _actionButton.interactable = false;
                return;
            }

            // 보유 → 강화(골드)
            int upCost = PieceQuery.GetNextUpgradeCost(_selName, _selGrade, level);
            ShowAction("강화하기", _goldIcon, reward != null ? reward.Gold : 0, upCost);
        }

        // 비용 영역·버튼 라벨 세팅. 부족해도 버튼은 살려두고, 누르면 알림(기획: 부족 안내).
        private void ShowAction(string label, Sprite icon, int have, int need)
        {
            if (_costArea != null) _costArea.SetActive(true);
            if (_costIcon != null && icon != null) _costIcon.sprite = icon;
            if (_costText != null) _costText.text = $"{have}/{need}";

            if (_actionLabel != null) _actionLabel.text = label;
            if (_actionButton != null) _actionButton.interactable = true;
        }

        // 버튼 — 미보유면 해금, 보유면 강화.
        private void OnTapAction()
        {
            if (_selGrade < 0) return;

            if (!PieceInventory.IsOwned(_selName, _selGrade)) TryUnlock();
            else TryUpgrade();
        }

        // 해금 — 스타더스트 소비. 부족하면 알림.
        private void TryUnlock()
        {
            int cost = PieceQuery.GetUnlockCost(_selName, _selGrade);

            if (!RewardManager.Instance.ConsumeStardust(cost))
            {
                PopupManager.Instance.ShowAlert("스타더스트가 부족합니다.");
                return;
            }

            PieceInventory.Unlock(_selName, _selGrade);   // OnChanged → 목록·상세 갱신
        }

        // 강화 — 골드 소비. 부족하거나 최대면 막음.
        private void TryUpgrade()
        {
            int level = PieceInventory.GetLevel(_selName, _selGrade);
            int max = PieceQuery.GetMaxLevel(_selName, _selGrade);
            if (level >= max) return;

            int cost = PieceQuery.GetNextUpgradeCost(_selName, _selGrade, level);

            if (!RewardManager.Instance.ConsumeGold(cost))
            {
                PopupManager.Instance.ShowAlert("골드가 부족합니다.");
                return;
            }

            PieceInventory.LevelUp(_selName, _selGrade, max);   // OnChanged → 목록·상세 갱신
        }
    }
}
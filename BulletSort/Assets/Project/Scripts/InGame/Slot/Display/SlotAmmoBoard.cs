using InGame.Slot.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Slot
{
    // 슬롯 잔탄보드 — 가동 포탑의 총 그림(타입별) + 남은 잔탄 수(xN)를 표시(기획 4.6).
    //   프리팹: Fill_info(월드 캔버스) > Weapon(Image, 총 그림) + Bullet_Text(TMP, "xN").
    // 책임 분리 — SlotHealthBar·SlotRepairGauge처럼 표시만. 잔탄 관리는 포탑 영역,
    //   여긴 ITurretPresence에서 *읽어* 그림. 컨트롤러가 Fill_info 루트를 가동 유무로 ON/OFF.
    // 갱신 두 경로:
    //   - 총 그림 — 타입은 구조 변화(소환·승격) 때만 바뀌므로 컨트롤러가 SetTowerType으로 1회 세팅.
    //   - 잔탄 수 — 발사마다 줄어 구조 변화로 안 잡힘 → Update에서 presence.ActiveAmmoCurrent 폴링.
    //     (가동 포탑 있을 때만. CurrentAmmo가 이미 노출돼 읽기만 하면 됨 — 포탑 코드 무수정)
    // 작성자: 이성규
    public class SlotAmmoBoard : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("잔탄보드 루트(Fill_info) — 노란 배경 포함, 가동 포탑 있을 때만 ON. " +
                 "컴포넌트는 이 위(부모)에 두고 이 오브젝트를 토글 — 꺼져도 Update 폴링은 유지")]
        [SerializeField] private GameObject _fillInfo;

        [Tooltip("총 그림 — 타입별 무기 스프라이트(월드 캔버스 Image). 비우면 Awake에서 탐색")]
        [SerializeField] private Image _weapon;

        [Tooltip("잔탄 수 텍스트(\"xN\"). 비우면 Awake에서 탐색")]
        [SerializeField] private TMP_Text _bulletText;

        // 잔탄 폴링 입력 — 컨트롤러가 주입(SlotTurretQueue). null이면 폴링 건너뜀.
        private ITurretPresence _presence;

        // 마지막으로 표시한 잔탄 수 — 매 프레임 문자열 생성·할당을 막는 캐시(변할 때만 갱신).
        private int _shownAmmo = -1;

        private void Awake()
        {
            if (_weapon == null)
                _weapon = GetComponentInChildren<Image>(includeInactive: true);
            if (_bulletText == null)
                _bulletText = GetComponentInChildren<TMP_Text>(includeInactive: true);
        }

        // 잔탄 폴링 입력 주입 — 컨트롤러가 presence(큐) 전달.
        public void SetPresence(ITurretPresence presence)
        {
            _presence = presence;
        }

        // 잔탄보드 표시 — 가동 포탑 타입(1~6)이면 Fill_info ON + 총 그림 세팅, 0이면 OFF(비워짐).
        //   컨트롤러가 가동 유무로 호출. 잔탄 수(xN)는 ON 상태에서 Update가 폴링.
        public void Show(int towerType)
        {
            bool on = towerType > 0;
            if (_fillInfo != null) _fillInfo.SetActive(on);

            if (on)
            {
                SetTowerType(towerType);
            }
            else
            {
                _shownAmmo = -1;   // 다음 ON 때 첫 폴링이 강제 갱신되도록 캐시 리셋
            }
        }

        // 총 그림 갱신 — 타입(1~6)에 맞는 무기 스프라이트로 교체. Show()에서만 호출.
        private void SetTowerType(int towerType)
        {
            if (_weapon == null) return;

            Sprite sprite = towerType > 0 ? SlotQuery.GetAmmoBoardIcon(towerType) : null;
            _weapon.sprite = sprite;
            _weapon.enabled = sprite != null;
        }

        // 잔탄 수 폴링 — 가동 포탑의 현재 잔탄을 읽어 "xN" 갱신.
        //   발사마다 줄어드는 값이라 구조 변화 이벤트로 못 잡아 Update에서 매 프레임 확인.
        //   값이 변할 때만 텍스트 할당(GC·SetText 호출 절약).
        //   컴포넌트가 Fill_info 위에 있어 보드가 꺼져도 Update는 돌므로, 꺼진 동안은 폴링 스킵.
        private void Update()
        {
            if (_presence == null || _bulletText == null) return;
            if (_fillInfo != null && !_fillInfo.activeSelf) return;   // 보드 꺼짐 — 폴링 불필요

            int cur = _presence.ActiveAmmoCurrent;
            if (cur == _shownAmmo) return;   // 안 변했으면 스킵

            _shownAmmo = cur;
            _bulletText.text = $"x{cur}";
        }
    }
}
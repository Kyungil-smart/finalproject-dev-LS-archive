using Towers.Interface.Tower;
using UnityEngine;   // Debug.LogWarning용으로 유지

namespace InGame.Slot
{
    // 오버소팅(잔탄 과열 사격) 메커니즘 — 기획서 기반.
    // 큐가 "대기까지 찼는데 또 정렬 성공" 판정을 내리면 이 객체에 진입을 위임한다.
    //   여기는 "어떻게(탄배출 지시·락·pending 보관)"만 담당. "언제 승격·등록할지" 최종 동작은 큐가.
    // 큐 ↔ 오버소팅 경계:
    //   - 큐: _active/_queue 흐름·최종 판단·동작(승격·등록)
    //   - 오버소팅: 상태(IsActive)·락(IsLocked)·pending 보관·탄배출 지시·강제 정리
    // 일반 C# 클래스 — MonoBehaviour 아님. 큐가 직접 new로 소유하고 메서드로만 구동(씬에 컴포넌트로 안 붙임).
    //   현재 탄배출 루프는 포탑 쪽이 코루틴으로 돌고 여긴 지시만 → 여기서 코루틴 돌릴 일 없음.
    //   기획서 '최대 지속 시간/최대 발사 수' 같은 타이머가 여기로 오면, 큐에 코루틴 위임 또는 MonoBehaviour로 환원.
    // 작성자: 이성규
    public class SlotOverSorting
    {
        // 진행 중 여부 — 큐가 파괴 통보 받을 때 "탄배출 완료냐"를 이걸로 분기.
        public bool IsActive { get; private set; }

        // 슬롯 락 — 탄배출 동안 이동·정렬·터치·드래그 차단. 슬롯 입력/정렬이 이걸 봄.
        //   현재는 IsActive와 동일하지만, 락 구간을 발사 일부로 좁힐 여지를 위해 분리해 둠.
        public bool IsLocked => IsActive;

        // 탄배출 끝나면 대기로 들어올 새 포탑 — 진입 시 큐에서 받아 보관, 완료/정리 때 큐로 반환.
        private ITower _pending;

        // 오버소팅 진입 — 큐가 "둘 다 찼다" 판정 후 호출.
        //   activeTurret(기존 가동)에 탄배출 지시하고, newTurret(이번 정렬 새 포탑)은 보관.
        //   기존 가동은 슬롯에 남은 채(큐의 _active 유지) 잔탄 토함 → 소진 자가소멸 → 큐가 Complete 호출.
        public void Enter(ITower activeTurret, ITower newTurret)
        {
            _pending = newTurret;
            IsActive = true;

            // 기존 가동에 잔탄 과열 사격 지시(포탑 쪽 Towers.OnOverSorting — 공속0.1·피해반감).
            // ITower엔 OnOverSorting seam이 없어 Towers 캐스팅. 인터페이스에 올라오면 캐스팅 제거.
            if (activeTurret is Towers.Factory.Towers t)
                t.OnOverSorting();
            else
                Debug.LogWarning("[SlotOverSorting] 가동이 Towers 아님 — 탄배출 미지시.");
        }

        // 탄배출 완료 — 큐가 "오버소팅 중 가동 파괴 통보"를 받았을 때 호출.
        //   상태·락 해제하고, 보관해 둔 pending을 큐로 반환(큐가 대기로 등록).
        public ITower Complete()
        {
            ITower promoted = _pending;
            _pending = null;
            IsActive = false;
            return promoted;
        }

        // 강제 정리 — 슬롯 파괴/전투 종료 시 큐가 호출. 보관 pending을 반환(큐가 Despawn)하고 상태 리셋.
        //   진행 중이 아니면 null 반환·no-op.
        public ITower Clear()
        {
            ITower leftover = _pending;
            _pending = null;
            IsActive = false;
            return leftover;
        }
    }
}
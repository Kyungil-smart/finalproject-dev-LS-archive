using System.Collections.Generic;
using InGame.Slot;
using Monster.Controll;
using UnityEngine;

namespace Monster.Spawn
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField]private SlotBoardManager _slotBoardManager;
        private List<Slot> _slots;
        private SlotHealth _slotHealth;
        
        private Slot _target;
        public Slot Target { get => _target;}
        

        private void Awake()
        {
            _slots = new List<Slot>();
        }

        private void Start()
        {
            _slots = _slotBoardManager._slots;
            _target = SelectTarget();
        }

        public Slot SelectTarget()
        {
            if (_slotHealth != null)
                _slotHealth.OnDead -= DeadEvent;

            Slot atkTarget = null;
            float mindistance = float.MaxValue;

            foreach (Slot slot in _slots)
            {
                if (slot.Health.isDead) continue;
                float targetDistance = GetDistance(slot.transform.position);
                if (mindistance > targetDistance)
                {
                    atkTarget = slot;
                    mindistance = targetDistance;
                }
            }

            Debug.Log($"<color=Green>{mindistance}</color>");

            if (atkTarget == null) return null;

            _slotHealth = atkTarget.Health;
            _slotHealth.OnDead += DeadEvent;

            Debug.Log($"타겟선정 {atkTarget.gameObject.name}");

            return atkTarget;
        }

        private void DeadEvent(SlotHealth health)
        {
            Debug.Log("[TestMSP] Dead");
            Debug.Log($"Target {_target.gameObject.name} is Dead");
            _target = SelectTarget();

            // 모든 슬롯 파괴 — 더 줄 타겟 없음 = 게임오버 시점
            if (_target == null)
            {
                Debug.Log("[TestMSP] 모든 슬롯 파괴 — 게임오버");
                // 게임오버 이벤트 연결 (게임플로우)
                return; // 아래 몬스터 타겟 갱신 안 함
            }

            Debug.Log($"새 타겟 {_target.gameObject.name}");

            MonsterController[] monsters = GetComponentsInChildren<MonsterController>();
            foreach (var monster in monsters)
            {
                monster.target = _target;
            }
        }

        // 직선거리 구하기
        private float GetDistance(Vector3 target)
        {
            float distance = Vector3.Distance(transform.position, target);
            return Mathf.Abs(distance);
        }
    }
}

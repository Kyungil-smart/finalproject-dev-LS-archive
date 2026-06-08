using Core.Manager.SpawnManager;
using InGame.Slot;
using System.Collections.Generic;
using UnityEngine;

public class TestMonsterSpawner : MonoBehaviour
{
    [SerializeField] GameObject monsterPrefab;

    [SerializeField] private SlotBoardManager _slotBoardManager;
    private List<Slot> _slots = new List<Slot>();

    public List<Slot> Slots => _slots;


    private int _spawnCount;
    private int _maxMonsterCount;

    private Timer _spawnTimer;
    private float _spawnLate;
    private Slot _target;
    private SlotHealth _slotHealth;

    private void Awake()
    {
        _spawnCount = 0;
        _maxMonsterCount = 10;
        _spawnLate = 1f;
        _spawnTimer = new Timer(_spawnLate);
    }

    private void Start()
    {
        _slots = _slotBoardManager._slots;
        _target = SelectTarget();
    }

    void FixedUpdate()
    {
        if (_target == null) return;
        if (_spawnCount >= _maxMonsterCount) return;

        _spawnTimer.UpdateTimer();

        if (_spawnTimer.IsEnabled)
        {
            GameObject spawnObj = Instantiate(monsterPrefab, transform.position, transform.rotation);
            spawnObj.transform.parent = transform;
            spawnObj.GetComponentInChildren<TestMonsterMove>().target = _target.gameObject;
            SpawnManager.Instance.monsters.Add(spawnObj);

            _spawnTimer.ResetTimer(_spawnLate);
            _spawnCount++;
        }
    }

    // 추후 조건 구현
    // 기본은 가까운 거리
    public Slot SelectTarget()
    {
        if (_slotHealth != null)
            _slotHealth.OnDead -= DeadEvent;

        Slot atkTarget = null; //_slots[0];
        float mindistance = 99999.0f; // GetDistance(atkTarget.transform.position);

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


        if (atkTarget != null)
        {
            _slotHealth = atkTarget.Health;

            _slotHealth.OnDead += DeadEvent;

            Debug.Log($"타겟선정 {atkTarget?.gameObject.name}");
        }

        return atkTarget;
    }

    private void DeadEvent(SlotHealth health)
    {
        Debug.Log("[TestMSP] Dead");
        Debug.Log($"Target {_target.gameObject.name} is Dead");
        _target = SelectTarget();
        TestMonsterMove[] monsters = GetComponentsInChildren<TestMonsterMove>();
        foreach (var monster in monsters)
        {
            monster.target = _target.gameObject;
        }
    }

    // 직선거리 구하기
    private float GetDistance(Vector3 target)
    {
        float distance = Vector3.Distance(transform.position, target);
        return Mathf.Abs(distance);
    }
}

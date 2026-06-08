using System.Collections.Generic;
using Core.Manager.SpawnManager;
using InGame.Slot;
using UnityEngine;

public class TestMonsterSpawner : MonoBehaviour
{
    [SerializeField] GameObject monsterPrefab;
    
    [SerializeField] private SlotBoardManager _slotBoardManager;
    private List<Slot> _slots = new List<Slot>();
    

    private int _spawnCount;
    private int _maxMonsterCount;

    private Timer _spawnTimer;
    private float _spawnLate;
    private Slot _target;

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
        SelectTarget();
    }

    void FixedUpdate()
    {
        if (_target == null) return;
        if (_spawnCount >= _maxMonsterCount) return;
            
        _spawnTimer.UpdateTimer();
        
        if (_spawnTimer.IsEnabled)
        {
            GameObject spawnObj = Instantiate(monsterPrefab, transform.position, transform.rotation);
            spawnObj.GetComponentInChildren<TestMonsterMove>().target = _target.gameObject;
            SpawnManager.Instance.monsters.Add(spawnObj);
            
            _spawnTimer.ResetTimer(_spawnLate);
            _spawnCount++;
        }
    }

    // 추후 조건 구현
    // 기본은 가까운 거리
    private void SelectTarget()
    {
        Slot atkTarget = _slots[0];
        float mindistance = GetDistance(atkTarget.transform.position);
        Debug.Log($"<color=Green>{mindistance}</color>");
      
        foreach (Slot slot in _slots)
        {
            float targetDistance = GetDistance(slot.transform.position);
            if (mindistance > targetDistance)
            {
                atkTarget = slot;
                mindistance = targetDistance;
            }
        }

        _target = atkTarget;
        Debug.Log("타겟선정");
    }
    
    // 직선거리 구하기
    private float GetDistance(Vector3 target)
    {
        float distance = Vector3.Distance(transform.position , target);
        return Mathf.Abs(distance);
    }
}

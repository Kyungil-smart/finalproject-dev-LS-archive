using Core.Manager.SpawnManager;
using UnityEngine;

public class TestMonsterSpawner : MonoBehaviour
{
    [SerializeField] GameObject monsterPrefab;
    [SerializeField] GameObject tower;

    private int _spawnCount;
    private int _maxMonsterCount;

    private Timer _spawnTimer;
    private float _spawnLate;

    private void Awake()
    {
        _spawnCount = 0;
        _maxMonsterCount = 10;
        _spawnLate = 1f;
        _spawnTimer = new Timer(_spawnLate);
    }

    void FixedUpdate()
    {
        if (_spawnCount >= _maxMonsterCount) return;
            
        _spawnTimer.UpdateTimer();
        
        if (_spawnTimer.IsEnabled)
        {
            GameObject spawnObj = Instantiate(monsterPrefab, transform.position, transform.rotation);
            spawnObj.GetComponent<TestMonsterMove>().target = tower;
            SpawnManager.Instance.monsters.Add(spawnObj);
            
            _spawnTimer.ResetTimer(_spawnLate);
            _spawnCount++;
        }
    }
}

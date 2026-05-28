using System.Collections.Generic;
using UnityEngine;

public class TestMonsterSpawner : MonoBehaviour
{
    [SerializeField] GameObject monsterPrefab;
    [SerializeField] GameObject tower;

    private int _maxMonsterCount;

    private Timer _spawnTimer;
    private float _spawnLate;
    
    public List<GameObject> monsters;

    private void Awake()
    {
        _maxMonsterCount = 10;
        _spawnLate = 1f;
        _spawnTimer = new Timer(_spawnLate);
        
        monsters = new List<GameObject>();
    }

    void Update()
    {
        if (monsters.Count >= _maxMonsterCount) return;
            
        _spawnTimer.UpdateTimer();
        
        if (_spawnTimer.IsEnabled)
        {
            GameObject spawnObj = Instantiate(monsterPrefab, transform.position, transform.rotation);
            spawnObj.GetComponent<TestMonsterMove>().target = tower;
            spawnObj.GetComponent<TestMonsterMove>()._spawnPoint = gameObject;
            monsters.Add(spawnObj);
            
            _spawnTimer.ResetTimer(_spawnLate);
        }
    }
}

using Towers.Factory;
using Towers.Factory.Type;
using UnityEngine;

public class TestTowerSpawner : MonoBehaviour
{
    [SerializeField] private TowerFactory _towerFactory;

    private void Start()
    {
        _towerFactory.CreateTower(ETowerType.Basic, gameObject.transform.position);
    }
}

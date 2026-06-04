using Core.Manager.SpawnManager;
using Towers.Factory.Type;
using UnityEngine;

public class TestTowerSpawner : MonoBehaviour
{
    private void Start()
    {
        SpawnManager.Instance.SpawnTower(ETowerType.Basic, gameObject.transform.position);
    }
}

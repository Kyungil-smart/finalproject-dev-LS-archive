using System.Collections.Generic;
using Towers;
using Towers.Factory;
using Towers.Factory.Type;
using UnityEngine;

namespace Core.Manager.SpawnManager
{
   public class SpawnManager : Singleton<SpawnManager>
   {
      [SerializeField] private TowerFactory _towerFactory;
      [SerializeField] private ProjectileFactory _projectileFactory;
      [SerializeField] private MonsterFactory _monsterFactory;
      
      public TowerFactory TowerFactory => _towerFactory;
      public ProjectileFactory ProjectileFactory => _projectileFactory;
      
      public List<GameObject> monsters;

      // 3sorting시 호출하여 타워 스폰
      public void SpawnTower(ETowerType towerType, Vector3 spawnPoint)
      {
         _towerFactory.CreateTower(towerType, spawnPoint);
      }

      public GameObject SpawnProjectile(EProjectileType projectileType, int count)
      {
        GameObject projectile = _projectileFactory.CreateProjectile(projectileType, count);

        return projectile;
      }
      
   }
}
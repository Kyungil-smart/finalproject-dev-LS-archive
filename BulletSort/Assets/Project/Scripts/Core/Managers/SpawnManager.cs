using System;
using System.Collections.Generic;
using InGame.Slot;
using Towers;
using Towers.Factory;
using Towers.Factory.Type;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Manager.SpawnManager
{
   public class SpawnManager : Singleton<SpawnManager>
   {
      [SerializeField] private TowerFactory _towerFactory;
      [SerializeField] private ProjectileFactory _projectileFactory;
      [SerializeField] private MonsterFactory _monsterFactory;
      
      private List<GameObject> _monsters;
      
      public TowerFactory TowerFactory => _towerFactory;
      public ProjectileFactory ProjectileFactory => _projectileFactory;
      public List<GameObject> Monsters => _monsters;
      

      private void OnEnable()
      {
         SceneManager.sceneLoaded += OnSceneLoaded;
      }

      private void OnDisable()
      {
         SceneManager.sceneLoaded -= OnSceneLoaded;
      }

      private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
      {
         InitScene(scene.name);
      }

      void InitScene(string sceneName)
      {
         // monsters 리스트가 null일 때만 새로 생성
         _monsters ??= new List<GameObject>(); 
         _monsters.Clear();
         
         if (sceneName == "InGame")
         {
            if (_towerFactory == null) 
               _towerFactory = FindFirstObjectByType(typeof(TowerFactory)) as TowerFactory;
            if(_projectileFactory == null)
               _projectileFactory = FindFirstObjectByType(typeof(ProjectileFactory)) as ProjectileFactory;
            if (_monsterFactory == null)
               _monsterFactory = FindFirstObjectByType(typeof(MonsterFactory)) as MonsterFactory;
         }
         else
         {
           _towerFactory = null;
           _projectileFactory = null;
           _monsterFactory = null;
         }
      }


      // 3sorting시 호출하여 타워 스폰
      public void SpawnTower(ETowerType towerType, Slot slot)
      {
         Transform spawnTr = slot.transform;
         _towerFactory.CreateTower(towerType, spawnTr);
      }

      public GameObject SpawnProjectile(EProjectileType projectileType, int count)
      {
        GameObject projectile = _projectileFactory.CreateProjectile(projectileType, count);

        return projectile;
      }
      
   }
}
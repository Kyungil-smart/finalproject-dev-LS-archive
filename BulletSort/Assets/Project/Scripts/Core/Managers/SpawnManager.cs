using System.Collections.Generic;
using InGame.Slot;
using Monster.Factory;
using Monster.Portal;
using Monster.Spawn;
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

      [SerializeField]private Portal[]  _portals; 
      [SerializeField] private Portal _topPortal;
      [SerializeField] private Portal _bottomPortal;
      
      private SpawnPoint[] _topSpawnPoints;
      private SpawnPoint[] _bottomSpawnPoints;
      
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
            if (_portals == null)
               _portals = FindObjectsByType(typeof(Portal), FindObjectsSortMode.None) as Portal[]; 
            
            if(_topPortal == null && _bottomPortal == null)
            {
               foreach (Portal portal in _portals)
               {
                  if(portal.transform.position.y > 0) _topPortal = portal;
                  
                  else if(portal.transform.position.y < 0) _bottomPortal = portal;
               }

               _topSpawnPoints = _topPortal.spawnPoints;
               _bottomSpawnPoints = _bottomPortal.spawnPoints;
            } 
         }
         else
         {
           _towerFactory = null;
           _projectileFactory = null;
           _monsterFactory = null;
           _portals = null;
           _topPortal = null;
           _bottomPortal = null;
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

      public void SpawnMonster(int spawnCount)
      {
         
         for (int i = 0; i < spawnCount; i++)
         {
            // 보스웨이브가 아닐 때
            //if()
            SpawnPoint spawnTr = RandomPoint();
            _monsterFactory.CreateMonster(spawnTr);
            
            //보스 웨이브 일 때
            //int index = Random.Range(0, _topSpawnPoints.Length);
            //_monsterFactory.CreateMonster(_topSpawnPoints[index]);

            Debug.Log("몬스터 생성");
         }
      }

      private SpawnPoint RandomPoint()
      {
          int topbottom = Random.Range(0, 1);
          Debug.Log($"상하단 넘버 : {topbottom}");
          int index = Random.Range(0, _topSpawnPoints.Length);
          Debug.Log($"스폰포인트 넘버 : {index}");  
          if (topbottom == 0) return _topSpawnPoints[index];
          
          return _bottomSpawnPoints[index];
      }
   }
}
using System.Collections.Generic;
using InGame.Slot;
using Monster.Factory;
using Monster.Portal;
using Monster.Spawn;
using Towers;
using Towers.Factory;
using Towers.Interface.Tower;
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
      
      public Portal[] Portals => _portals;
      
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

      public void WaveStart()
      {
         _topPortal.StartSpawn();
      }

      public void WaveEnd()
      {
         _topPortal.ResetPhase();
      }

      // 3sorting시 호출하여 타워 스폰
      public ITower SpawnTower(int towerID, Slot slot)
      {
         Transform spawnTr = slot.transform;
         return _towerFactory.CreateTower(towerID, spawnTr);
      }

      public GameObject SpawnProjectile(EProjectileType projectileType, int count)
      {
        GameObject projectile = _projectileFactory.CreateProjectile(projectileType, count);

        return projectile;
      }

      public void SpawnMonster(int monsterGroupID, int spawnCount)
      {
         MonsterGroupData groupdata = DataManager.Instance.GetData<MonsterGroupData>(monsterGroupID);
         {
            for (int i = 0; i < spawnCount; i++)
            {
               int monsterID = RandomID(groupdata);
               SpawnPoint spawnTr = RandomPoint();
               _monsterFactory.CreateMonster(spawnTr, monsterID);
            }
         }
      }

      public void SpawnBoss()
      {
         //보스 웨이브 일 때
         if (StageManager.Instance.IsBossWave)
         {
            _monsterFactory.CreateBoss(_topPortal.bossZone, StageManager.Instance.BossID);
         }
      }

      private SpawnPoint RandomPoint()
      {
          int topbottom = Random.Range(0, 10);
          int index = Random.Range(0, _topSpawnPoints.Length);
          if (topbottom < 5) return _topSpawnPoints[index];
          
          return _bottomSpawnPoints[index];
      }

      private int RandomID(MonsterGroupData groupData)
      {
         List<int> ids = new List<int>();

         AddList(groupData.MonsterID_1, groupData.NormalRate_1, ids);
         AddList(groupData.MonsterID_2, groupData.NormalRate_2, ids);
         AddList(groupData.MonsterID_3, groupData.NormalRate_3, ids);

         int index = Random.Range(0, ids.Count);
         
         int id = ids[index];  

         return id;
      }

      private void AddList(int monsterID, int Rate, List<int> monsterids)
      {
         if (monsterID == 0 || Rate == 0) return;
         
         for (int i = 0; i < Rate; i++)
         {
            monsterids.Add(monsterID);
         }
      }
   }
}
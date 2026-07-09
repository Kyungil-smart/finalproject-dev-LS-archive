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
      private List<int> _monsterIDs;
      
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
            _towerFactory = FindFirstObjectByType(typeof(TowerFactory)) as TowerFactory;
            _projectileFactory = FindFirstObjectByType(typeof(ProjectileFactory)) as ProjectileFactory;
            _monsterFactory = FindFirstObjectByType(typeof(MonsterFactory)) as MonsterFactory;
            _portals = FindObjectsByType(typeof(Portal), FindObjectsSortMode.None) as Portal[]; 
            
            if(_topPortal == null || _bottomPortal == null)
            {
               foreach (Portal portal in _portals)
               {
                  if(portal.transform.position.y > 0) _topPortal = portal;
                  
                  else if(portal.transform.position.y < 0) _bottomPortal = portal;
               }
            }

            _topSpawnPoints = _topPortal.spawnPoints;
            _bottomSpawnPoints = _bottomPortal.spawnPoints;
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
               int monsterID = groupdata.MonsterID_1;
               if(DataManager.Instance.GetData<MonsterData>(monsterID).MonsterType == 1)
               {  monsterID = RandomID(groupdata); }

               else
               {
                  monsterID = groupdata.MonsterID_1 != 0 ?  groupdata.MonsterID_1 : groupdata.MonsterID_2  ;
                  
                  if(monsterID != 0) return;
                  
                  monsterID = monsterID != 0 ?  groupdata.MonsterID_2 : groupdata.MonsterID_3;
               }
               
               if(monsterID == 0) continue;
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
         _monsterIDs = new List<int>();

         AddList(groupData.MonsterID_1, StageManager.Instance.CurWavePattern.NormalRate_1);
         AddList(groupData.MonsterID_2, StageManager.Instance.CurWavePattern.NormalRate_2);
         AddList(groupData.MonsterID_3, StageManager.Instance.CurWavePattern.NormalRate_3);

         int index = Random.Range(0, _monsterIDs.Count);
         
         int id = _monsterIDs[index];  

         return id;
      }

      private void AddList(int monsterID, int Rate)
      {
         if (monsterID == 0 || Rate == 0) return;
         
         for (int i = 0; i < Rate; i++)
         {
            _monsterIDs.Add(monsterID);
         }
      }
   }
}
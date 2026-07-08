using Core;
using Core.Manager.SpawnManager;
using Monster.Controll;
using Monster.Spawn;
using UnityEngine;

namespace Monster.Factory
{
    
    public class MonsterFactory : MonoBehaviour
    {
        [SerializeField] private GameObject _bossPrefab;
        [SerializeField] private MonsterPrefabTable _monsterPrefabTable;
        // 몬스터 오브젝트 생성
        public void CreateMonster(SpawnPoint spawnPoint , int monsterID)
        {
            GameObject instance = new GameObject();
            MonsterController monsterctr = instance.AddComponent<MonsterController>();
            instance.AddComponent<CircleCollider2D>();
            
            // 몬스터 데이터 불러오기
            MonsterData monsterData = DataManager.Instance.GetData<MonsterData>(monsterID);
            // 몬스터 데이터 저장하기
            // 체력, 공격력, 공격속도, 이동속도, 경험치
            monsterctr.Init(monsterData);

            instance.name = monsterData.name;
            instance.tag = "Monster";
            Instantiate(_monsterPrefabTable.GetByName(monsterData.MonsterSprite), instance.transform); 
            
            SpriteRenderer[] spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
            
            foreach (SpriteRenderer sprite in spriteRenderer)
                sprite.sortingLayerName = "Monster";

            var pos = spawnPoint.transform.position;
            
            instance.transform.position = pos;
            instance.transform.SetParent(spawnPoint.transform);
            monsterctr.target = spawnPoint.Target;
            SpawnManager.Instance.Monsters.Add(instance);
        }

        public void CreateBoss(Transform spawnPoint, int bossID)
        {
            MonsterData bossData = DataManager.Instance.GetData<MonsterData>(bossID);
            GameObject instance = Instantiate(_bossPrefab, spawnPoint);
            instance.GetComponent<MonsterController>().Init(bossData);
        }
    }
}

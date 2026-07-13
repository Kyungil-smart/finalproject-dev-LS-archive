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
        [SerializeField] private MonsterSpriteTable _monsterSpriteTable;
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
            var pos = spawnPoint.transform.position;
            instance.transform.position = pos;
            instance.transform.SetParent(spawnPoint.transform);
            
            GameObject child = new GameObject();
            child.name = monsterData.MonsterSprite;
            child.transform.SetParent(instance.transform);
            child.transform.position = pos;
            SpriteRenderer sprite = child.AddComponent<SpriteRenderer>();
            sprite.sprite = _monsterSpriteTable.GetByName(monsterData.MonsterSprite);
            
            if(sprite.sprite!= null)
                sprite.sortingLayerName = "Monster";
            
            monsterctr.target = spawnPoint.Target;
            SpawnManager.Instance.Monsters.Add(instance);
        }

        public void CreateBoss(Transform spawnPoint, int bossID)
        {
            MonsterData bossData = DataManager.Instance.GetData<MonsterData>(bossID);
            GameObject instance = Instantiate(_bossPrefab, spawnPoint);
            
            GameObject child = Instantiate(new GameObject(), instance.transform);
            SpriteRenderer sprite = child.AddComponent<SpriteRenderer>();

            sprite.sprite = _monsterSpriteTable.GetByName(bossData.MonsterSprite);
            
            if(sprite.sprite!= null)
                sprite.sortingLayerName = "Monster";
            
            instance.GetComponent<MonsterController>().Init(bossData);
        }
    }
}

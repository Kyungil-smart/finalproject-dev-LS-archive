using Core;
using Core.Manager.SpawnManager;
using Monster.Boss;
using Monster.Controll;
using Monster.Spawn;
using UnityEngine;

namespace Monster.Factory
{
    
    public class MonsterFactory : MonoBehaviour
    {
        [SerializeField] private GameObject _bossPrefab;
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
            SpriteRenderer spriteRenderer = instance.gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = "Monster";
            int monsterType = monsterData.MonsterType;
            // 타입에 따른 스프라이트 설정
            switch (monsterType)
            {
                case 1:
                    spriteRenderer.sprite =
                        Resources.Load<Sprite>("Test_Monster_Sprites/Test_NormalMonster_Sprite");
                    break;
                case 2:
                    spriteRenderer.sprite =
                        Resources.Load<Sprite>("Test_Monster_Sprites/Test_SpeedyMonster_Sprite");
                    break;
                case 3:
                    spriteRenderer.sprite =
                        Resources.Load<Sprite>("Test_Monster_Sprites/Test_TankMonster_Sprite");
                    break;
            }

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

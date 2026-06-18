using Core;
using Core.Manager.SpawnManager;
using Monster.Controll;
using Monster.Spawn;
using UnityEngine;

namespace Monster.Factory
{
    public class MonsterFactory : MonoBehaviour
    {
        // 몬스터 오브젝트 생성
        public void CreateMonster(SpawnPoint spawnPoint)
        {
            GameObject instance = new GameObject();
            MonsterController monsterctr = instance.AddComponent<MonsterController>();
            
            // 몬스터 데이터 불러오기
            MonsterData monsterData = DataManager.Instance.GetData<MonsterData>(04111);
            // 몬스터 데이터 저장하기
            // 체력, 공격력, 공격속도, 이동속도, 경험치
            monsterctr.Init(monsterData);

            // 임시 코드
            instance.name = monsterData.name;
            SpriteRenderer spriteRenderer = instance.gameObject.AddComponent<SpriteRenderer>();

            // 스프라이트 설정을 위한 스위치문
            int monsterType = monsterData.MonsterID%10;
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
            monsterctr.target = spawnPoint.Target;
            SpawnManager.Instance.Monsters.Add(instance);
            Debug.Log("몬스터오브젝트생성");
        }
    }
}

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

            // 몬스터 데이터 저장하기
            // 체력, 공격력, 공격속도, 이동속도, 경험치
            monsterctr.Init();

            // 임시 코드
            instance.name = "Normal";

            // 스프라이트 설정을 위한 스위치문
            //int monsterType = MonsterID%10;
            // 임시코드
            int monsterType = 1;

            switch (monsterType)
            {
                case 1:
                    //Normal
                    //instance.gameObject.GetComponent<SpriteRenderer>().sprite = 
                    break;
                case 2:
                    //Speedy
                    break;
                case 3:
                    //Tank
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

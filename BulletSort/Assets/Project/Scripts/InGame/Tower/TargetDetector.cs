using Core;
using Core.Manager.SpawnManager;
using Monster.Controll;
using UnityEngine;

public class TargetDetector : MonoBehaviour
{
   [Header("사거리"), SerializeField]
   private float _bulletDuration;

   private float _bulletSpeed;
   private float _detectRange;
   
   [SerializeField]public GameObject target;
   [SerializeField]public List<GameObject> _detectedMonsters;
   

   private void Awake()
   {
      _bulletSpeed = 1f;
      _detectedMonsters = new List<GameObject>();
   }

   private void Start()
   {
      _detectRange = _bulletSpeed * _bulletDuration;
   }

   private void Update()
   {
      if(target != null && target.GetComponent<MonsterController>().isDead)
         KillTarget();
   }

   private void FixedUpdate()
   {
      // 생성된 몬스터가 없다면 탐색 X
      if(SpawnManager.Instance.Monsters.Count <=0) return;
      
      if (target != null) return;
      
      DetectTarget();
   }

   // 단순 거리 대조 방식
   private void DetectTarget()
   {
      // 테스트용
      List<GameObject> monster = SpawnManager.Instance.Monsters;
      
      foreach (GameObject enemy in monster)
      {
         if (enemy == null) continue;
         // 포탑과 몬스터 사이의 거리 계산
         float distance = GetDistance(enemy.transform.position);
         Debug.Log($"<color=Red>{distance}</color>");
         
         // 탐지된 적인지 확인
         if (_detectedMonsters.Contains(enemy))
         {
            if (distance < _detectRange) continue;
            // 사정거리 밖이면 삭제
            _detectedMonsters.Remove(enemy);
         }
         
         else
         {  
            // 거리가 사정거리 안 일 때 리스트에 추가
            if (distance < _detectRange)
            { _detectedMonsters.Add(enemy); }
         }
      }
      
      // 탐지된 적이 없다면 다시 탐지
      if(_detectedMonsters.Count == 0) return;
      
      SelectTarget();
   }

   // 추후 조건 구현
   // 기본은 가까운 거리
   private void SelectTarget()
   {
      GameObject atkTarget = null;
      float mindistance = float.MaxValue;
      Debug.Log($"<color=Green>{mindistance}</color>");
      
      foreach (GameObject monster in _detectedMonsters)
      {
         if (monster == null) continue;
         float targetDistance = GetDistance(monster.transform.position);
         if (mindistance > targetDistance)
         {
            atkTarget = monster;
            mindistance = targetDistance;
         }
      }
      
      if (atkTarget == null) return;

      target = atkTarget;
      Debug.Log("타겟선정");
   }

   private void KillTarget()
   {
      Debug.Log("목표제거");
      _detectedMonsters.Remove(target);
      Destroy(target);
      target = null;
   }
   
   // 직선거리 구하기
   private float GetDistance(Vector3 target)
   {
      float distance = Vector3.Distance(transform.position , target);
      return Mathf.Abs(distance);
   }
}

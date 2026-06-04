using UnityEngine;

public class TestMonsterMove : MonoBehaviour
{
    private float _moveSpeed;
    
    public GameObject target;
    
    [SerializeField] public GameObject _spawnPoint;
    
    // 테스트 용 몹 디스트로이 타이머
    private Timer _deadTimer;

    private void Awake()
    {
        _moveSpeed = 2.5f;
        _deadTimer = new Timer(5f);
    }

    private void Update()
    {
        _deadTimer.UpdateTimer();
        if (_deadTimer.IsEnabled)
        {
            target.GetComponent<TargetDetector>().target = null;
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        Move();        
    }

    // 몬스터가 죽을 때 
    private void OnDestroy()
    {
        // 여기서 리스트에 직접접근(테스트용)
        target.GetComponent<TargetDetector>()._detectedMonsters.Remove(gameObject);
        _spawnPoint.GetComponent<TestMonsterSpawner>().monsters.Remove(gameObject);
        
        // 리스트를 관리하는 객체에서 일괄 처리(결합 시 구조설계필요)
    }
    
    // 몬스터가 넉백을 맞을 경우만 사정거리 밖으로 나감
    private void OnKnockBack()
    {
        
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position,
            target.transform.position, _moveSpeed * Time.deltaTime);
    }
}

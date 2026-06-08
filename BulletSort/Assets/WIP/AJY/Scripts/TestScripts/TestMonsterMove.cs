using Core.Interface.IDamageable;
using Core.Manager.SpawnManager;
using UnityEngine;

public class TestMonsterMove : MonoBehaviour, IDamageable
{
    private float _moveSpeed;
    private int atk;
    private int _atkSpeed;
    
    [SerializeField]private int _health;
    private int _maxHealth = 50;
    
    public bool isDead;
    
    private Timer atkTimer;
    
    public GameObject target;
    
    private void Awake()
    {
        _moveSpeed = 2.5f;
        atk = 10;
        _atkSpeed = 2;
        atkTimer = new Timer(_atkSpeed);
        
        _health = _maxHealth;
        
        isDead = false;
    }

    private void Update()
    {
        if(!atkTimer.IsEnabled)
            atkTimer.UpdateTimer();
    }

    private void FixedUpdate()
    {
        if(Mathf.Abs(Vector3.Distance(target.transform.position, gameObject.transform.position)) >= 1 )
            Move();
        
        else if (atkTimer.IsEnabled)
        {
            Attack();
            atkTimer.ResetTimer(_atkSpeed);
        }
    }

    // 몬스터가 죽을 때 
    private void OnDestroy()
    {
        Debug.Log(gameObject.name + " has been destroyed");
        // 리스트를 관리하는 객체에서 일괄 처리(결합 시 구조설계필요)
    }
    
    // 몬스터가 넉백을 맞을 경우만 사정거리 밖으로 나감
    private void OnKnockBack()
    {
        
    }

    private void Attack()
    {
        target.GetComponent<IDamageable>().TakeDamage(atk);
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position,
            target.transform.position, _moveSpeed * Time.deltaTime);
    }

    public int Health 
    { 
        get
        { return _health; }
        set
        { } 
    }
    public int MaxHealth
    {
        get
        { return _maxHealth; } 
        set
        {}
    }
    
    public void TakeDamage(int Damage)
    {
        _health -= Damage;

        if (_health <= 0)
        {
            Dead();
        }
    }
    
    public void Dead()
    {
        SpawnManager.Instance.Monsters.Remove(gameObject);
        isDead = true;
    }
}

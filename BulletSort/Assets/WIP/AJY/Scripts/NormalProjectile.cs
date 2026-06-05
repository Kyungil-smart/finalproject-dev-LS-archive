using Core.Interface.IDamageable;
using Core.Manager.StageDataManager;
using Core.ObjectPool;
using Projectile.Interface;
using Core.ObjectPool.Interface;
using UnityEngine;

public class NormalProjectile : MonoBehaviour, IProjectile, IPoolable
{
    private GameObject _target;
    private float _moveSpeed;
    private int _atk = 10;

    public GameObject Target
    {
        get { return _target; }
        set { _target = value; }
    }
    
    public int Atk
    {
        get { return _atk;}
        set{}
    }

    private void FixedUpdate()
    {
        MoveToTarget(_target);
    }
    
    public void MoveToTarget(GameObject target)
    {
        if (target == null) return;
        
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,
            _target.transform.position, _moveSpeed * Time.deltaTime);
        // 투사체가 몬스터에 도달 시
        AtkTarget(target);
    }

    public void AtkTarget(GameObject target)
    {
        // 피해 계산
        target.GetComponent<IDamageble>().TakeDamage(_atk);
        Destroy(gameObject);
    }

    public void OnSpawn()
    {
        Init();

        // 데이터 불러오기
    }

    // SO에서 데이터 받아오기
    private void Init()
    {  
        _moveSpeed = 10f;
    }

    public void OnDespawn()
    {
        // 풀로 돌아가기
        //PoolManager.Instance.Release();
    }
}

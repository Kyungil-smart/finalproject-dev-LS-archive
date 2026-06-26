using Core.Interface.IDamageable;
using Core.Manager.SpawnManager;
using InGame.Slot;
using UnityEngine;

namespace Monster.Controll
{
    public class MonsterController : MonoBehaviour, IDamageable
    {
        [Tooltip("몬스터 이동속도")]
        [SerializeField]private float _moveSpeed;
        
        [Tooltip("몬스터 공격력")]
        [SerializeField]private int _atk;
        
        [Tooltip("몬스터 공격속도")]
        [SerializeField]private float _atkSpeed;
        
        [Tooltip("몬스터 최대 체력")]
        [SerializeField]private int _maxHealth;
        
        [Tooltip("몬스터 현재 체력")]
        [SerializeField] private int _health;

        public bool isDead;

        private Timer atkTimer;

        public Slot target;

        private void Update()
        {
            if (!atkTimer.IsEnabled)
                atkTimer.UpdateTimer();
        }

        private void FixedUpdate()
        {
            if (Mathf.Abs(Vector3.Distance(target.transform.position, gameObject.transform.position)) >= 1)
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
        }

        // 몬스터가 넉백을 맞을 경우만 사정거리 밖으로 나감
        private void OnKnockBack()
        {

        }

        private void Attack()
        {
            target.GetComponent<IDamageable>().TakeDamage(_atk);
        }

        private void Move()
        {
            transform.position = Vector3.MoveTowards(transform.position,
                target.transform.position, _moveSpeed * Time.deltaTime);
        }

        public int Health
        {
            get { return _health; }
            set { }
        }

        public int MaxHealth
        {
            get { return _maxHealth; }
            set { }
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

        public void Init(MonsterData monsterData)
        {
            if (monsterData.MonsterType == 4) _moveSpeed = 0;
            
            else _moveSpeed = monsterData.MonsterMoveSpeed;
            
            _atk = monsterData.MonsterAtk;
            _atkSpeed = monsterData.MonsterAtkSpeed;
            atkTimer = new Timer(_atkSpeed);
            _maxHealth = monsterData.MonsterHp;

            _health = _maxHealth;

            isDead = false;
        }
    }

}
namespace Core.Interface.IDamageable
{
    public interface IDamageable
    {
        public int Health { get; }
        public int MaxHealth { get; }

        public void TakeDamage(int amount);
    }
}


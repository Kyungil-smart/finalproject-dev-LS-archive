namespace Core.Interface.IDamageable
{
    public interface IDamageble
    {
        public int Health { get; }
        public int MaxHealth { get; }

        public void TakeDamage(int amount);
    }
}


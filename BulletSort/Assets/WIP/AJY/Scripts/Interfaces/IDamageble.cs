namespace Core.Interface.IDamageble
{
    public interface IDamageble
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }

        public void Dead();
    }
}


namespace GameClient.Runtime.Events
{
    public readonly struct EnemyReachedMainTowerEvent
    {
        public readonly int Damage;

        public EnemyReachedMainTowerEvent(int damage)
        {
            Damage = damage;
        }
    }
}

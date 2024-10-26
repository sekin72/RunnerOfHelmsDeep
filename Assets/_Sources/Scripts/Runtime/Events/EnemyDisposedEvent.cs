namespace GameClient.Runtime.Events
{
    public readonly struct EnemyDisposedEvent
    {
        public readonly Enemy Enemy;

        public EnemyDisposedEvent(Enemy enemy)
        {
            Enemy = enemy;
        }
    }
}

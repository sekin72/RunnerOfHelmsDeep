namespace GameClient.Runtime.Events
{
    public readonly struct PlayerUnitCostChangedEvent
    {
        public readonly int PoolKey;
        public readonly int Cost;

        public PlayerUnitCostChangedEvent(int poolKey, int cost)
        {
            PoolKey = poolKey;
            Cost = cost;
        }
    }
}

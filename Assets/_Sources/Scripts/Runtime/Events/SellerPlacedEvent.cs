namespace GameClient.Runtime.Events
{
    public readonly struct SellerPlacedEvent
    {
        public readonly int PoolKey;
        public readonly PassiveTile PassiveTile;

        public SellerPlacedEvent(int poolKey, PassiveTile passiveTile)
        {
            PoolKey = poolKey;
            PassiveTile = passiveTile;
        }
    }
}

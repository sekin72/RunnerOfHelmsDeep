namespace GameClient.Runtime.Events
{
    public readonly struct PlayerUnitSoldEvent
    {
        public readonly int Cost;

        public PlayerUnitSoldEvent(int cost)
        {
            Cost = cost;
        }
    }
}

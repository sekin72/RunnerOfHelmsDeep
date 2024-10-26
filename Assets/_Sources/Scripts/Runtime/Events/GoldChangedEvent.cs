namespace GameClient.Runtime.Events
{
    public readonly struct GoldChangedEvent
    {
        public readonly int Gold;

        public GoldChangedEvent(int gold)
        {
            Gold = gold;
        }
    }
}

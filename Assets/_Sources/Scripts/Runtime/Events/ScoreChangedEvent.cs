namespace GameClient.Runtime.Events
{
    public readonly struct ScoreChangedEvent
    {
        public readonly int Score;

        public ScoreChangedEvent(int increaseScoreAmount)
        {
            Score = increaseScoreAmount;
        }
    }
}

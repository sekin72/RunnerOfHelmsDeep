using CerberusFramework.Utilities;

namespace CFGameClient
{
    public record GameSoundKeys : Enumeration<GameSoundKeys>
    {
        public static GameSoundKeys MainTheme = new(1000, nameof(MainTheme));
        public static GameSoundKeys Arrow = new(1001, nameof(Arrow));
        public static GameSoundKeys BattleBG = new(1002, nameof(BattleBG));
        public static GameSoundKeys BattleSwords = new(1003, nameof(BattleSwords));
        public static GameSoundKeys Victory = new(1004, nameof(Victory));
        public static GameSoundKeys Fireball = new(1005, nameof(Fireball));

        protected GameSoundKeys(int id, string name) : base(id, name)
        {
        }
    }
}
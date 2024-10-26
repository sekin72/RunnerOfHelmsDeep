using CerberusFramework.Utilities;

namespace CFGameClient
{
    public record GamePoolKeys : Enumeration<GamePoolKeys>
    {
        public static GamePoolKeys ExternalParticlesSystemView = new(500, nameof(ExternalParticlesSystemView));
        public static GamePoolKeys EnvironmentCreatorSystemView = new(501, nameof(EnvironmentCreatorSystemView));

        public static GamePoolKeys Tile = new(600, nameof(Tile));
        public static GamePoolKeys GroundTile = new(601, nameof(GroundTile));
        public static GamePoolKeys PlayerUnitTile = new(602, nameof(PlayerUnitTile));
        public static GamePoolKeys PathTile = new(603, nameof(PathTile));

        public static GamePoolKeys EnemySpawnGate = new(606, nameof(EnemySpawnGate));
        public static GamePoolKeys MainTower = new(607, nameof(MainTower));

        public static GamePoolKeys DefaultEnemy = new(608, nameof(DefaultEnemy));
        public static GamePoolKeys TinyEnemy = new(609, nameof(TinyEnemy));
        public static GamePoolKeys TrollEnemy = new(610, nameof(TrollEnemy));

        public static GamePoolKeys HumanArcher = new(612, nameof(HumanArcher));
        public static GamePoolKeys ElfArcher = new(613, nameof(ElfArcher));
        public static GamePoolKeys Wizard = new(614, nameof(Wizard));

        public static GamePoolKeys Arrow = new(650, nameof(Arrow));
        public static GamePoolKeys Fireball = new(651, nameof(Fireball));

        public static GamePoolKeys PausePopup = new(1000, nameof(PausePopup));
        public static GamePoolKeys WinPopupVariant = new(1001, nameof(WinPopupVariant));
        public static GamePoolKeys FailPopupVariant = new(1002, nameof(FailPopupVariant));
        protected GamePoolKeys(int id, string name) : base(id, name)
        {
        }
    }
}